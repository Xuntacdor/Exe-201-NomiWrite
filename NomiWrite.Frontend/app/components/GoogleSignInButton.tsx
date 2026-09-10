"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { AlertCircle, Globe2, Loader2 } from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { saveSession } from "@/lib/auth/session";

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (options: {
            client_id: string;
            callback: (response: { credential?: string }) => void;
          }) => void;
          renderButton: (
            parent: HTMLElement,
            options: {
              theme?: "outline" | "filled_blue" | "filled_black";
              size?: "large" | "medium" | "small";
              width?: number;
              text?: "signin_with" | "signup_with" | "continue_with";
              shape?: "rectangular" | "pill" | "circle" | "square";
            },
          ) => void;
        };
      };
    };
  }
}

interface GoogleSignInButtonProps {
  mode: "login" | "register";
}

const googleClientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID ?? "";
const googleScriptId = "google-identity-services";

function loadGoogleScript() {
  return new Promise<void>((resolve, reject) => {
    if (window.google?.accounts?.id) {
      resolve();
      return;
    }

    const existing = document.getElementById(googleScriptId) as HTMLScriptElement | null;
    if (existing) {
      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error("Could not load Google sign-in.")), { once: true });
      return;
    }

    const script = document.createElement("script");
    script.id = googleScriptId;
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("Could not load Google sign-in."));
    document.head.appendChild(script);
  });
}

export default function GoogleSignInButton({ mode }: GoogleSignInButtonProps) {
  const router = useRouter();
  const containerRef = useRef<HTMLDivElement | null>(null);
  const [status, setStatus] = useState<"loading" | "ready" | "error">(googleClientId ? "loading" : "error");
  const [message, setMessage] = useState("");

  useEffect(() => {
    if (!googleClientId) {
      return;
    }

    let cancelled = false;

    loadGoogleScript()
      .then(() => {
        if (cancelled || !containerRef.current || !window.google?.accounts?.id) return;

        window.google.accounts.id.initialize({
          client_id: googleClientId,
          callback: async response => {
            if (!response.credential) {
              setStatus("error");
              setMessage("Google did not return an id token.");
              return;
            }

            try {
              setStatus("loading");
              const session = await apiClient.googleLogin({ idToken: response.credential });
              saveSession(session);
              router.push("/dashboard");
            } catch (err) {
              setStatus("error");
              setMessage(err instanceof Error ? err.message : "Google sign-in failed.");
            }
          },
        });

        containerRef.current.innerHTML = "";
        window.google.accounts.id.renderButton(containerRef.current, {
          theme: "outline",
          size: "large",
          width: containerRef.current.clientWidth || 320,
          text: mode === "register" ? "signup_with" : "continue_with",
          shape: "rectangular",
        });
        setStatus("ready");
      })
      .catch(err => {
        if (!cancelled) {
          setStatus("error");
          setMessage(err instanceof Error ? err.message : "Could not load Google sign-in.");
        }
      });

    return () => {
      cancelled = true;
    };
  }, [mode, router]);

  if (!googleClientId) {
    return (
      <div className="mb-6">
        <button
          type="button"
          disabled
          title="Set NEXT_PUBLIC_GOOGLE_CLIENT_ID to enable Google sign-in"
          className="mb-2 flex w-full cursor-not-allowed items-center justify-center gap-3 rounded-xl bg-white/70 py-3 text-sm font-semibold text-slate-500 shadow-sm"
        >
          <Globe2 className="h-4 w-4 text-blue-500" />
          {mode === "register" ? "Sign up with Google" : "Continue with Google"}
        </button>
        <p className="text-center text-[11px] font-medium text-slate-500">
          Google sign-in needs NEXT_PUBLIC_GOOGLE_CLIENT_ID.
        </p>
      </div>
    );
  }

  return (
    <div className="mb-6">
      <div ref={containerRef} className="min-h-11 w-full overflow-hidden rounded-xl bg-white" />
      {status === "loading" && (
        <div className="mt-2 flex items-center justify-center gap-2 text-[11px] font-medium text-slate-500">
          <Loader2 className="h-3 w-3 animate-spin" />
          Connecting to Google
        </div>
      )}
      {status === "error" && message && (
        <div className="mt-2 flex items-start gap-2 rounded-xl border border-amber-400/30 bg-amber-500/10 px-3 py-2 text-[11px] font-medium text-amber-100">
          <AlertCircle className="mt-0.5 h-3.5 w-3.5 shrink-0" />
          <span>{message}</span>
        </div>
      )}
    </div>
  );
}
