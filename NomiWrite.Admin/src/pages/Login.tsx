import axios from "axios";
import { useState } from "react";
import type { FormEvent } from "react";
import { Lock, Mail } from "lucide-react";
import { Navigate, useNavigate } from "react-router-dom";
import { hasAdminSession, isAdminRole, saveAdminSession } from "../lib/authSession";
import AdminSettingsButton from "../components/AdminSettingsButton";

interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  fullName: string;
  role: string;
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5097";

export default function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  if (hasAdminSession()) {
    return <Navigate to="/" replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");

    try {
      setSubmitting(true);
      const response = await axios.post<AuthResponse>(`${apiBaseUrl}/api/auth/login`, {
        email: email.trim(),
        password,
      });

      if (!isAdminRole(response.data.role)) {
        setError("This account does not have access to the admin panel.");
        return;
      }

      saveAdminSession(response.data);
      navigate("/", { replace: true });
    } catch (err) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message ?? "Login failed. Please check your credentials.");
      } else {
        setError("Login failed. Please try again.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="admin-login flex min-h-screen items-center justify-center px-4">
      <AdminSettingsButton floating />
      <div className="admin-login-card w-full max-w-sm rounded-2xl border p-7 shadow-2xl">
        <div className="mb-7 flex items-center gap-3">
          <img src="/nomiwrite-mark.svg" alt="" aria-hidden="true" className="h-11 w-11 shrink-0" />
          <div>
            <h1 className="text-lg font-extrabold">NomiWrite Admin</h1>
            <p className="text-xs font-medium text-[var(--admin-muted)]">Sign in with an admin account</p>
          </div>
        </div>

        <form className="space-y-4" onSubmit={handleSubmit}>
          <div>
            <label className="mb-1.5 block text-xs font-semibold">Email</label>
            <div className="relative">
              <Mail className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--admin-muted)]" />
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                className="admin-input w-full rounded-xl border py-3 pl-10 pr-3 text-sm outline-none transition"
              />
            </div>
          </div>

          <div>
            <label className="mb-1.5 block text-xs font-semibold">Password</label>
            <div className="relative">
              <Lock className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-[var(--admin-muted)]" />
              <input
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                className="admin-input w-full rounded-xl border py-3 pl-10 pr-3 text-sm outline-none transition"
              />
            </div>
          </div>

          {error && (
            <p className="rounded-xl border border-red-500/40 bg-red-500/10 px-3 py-2 text-xs font-semibold text-red-200">
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={submitting}
            className="admin-primary-button w-full rounded-xl py-3 text-sm font-extrabold transition disabled:cursor-not-allowed disabled:opacity-60"
          >
            {submitting ? "Signing in..." : "Sign in"}
          </button>
        </form>
      </div>
    </div>
  );
}
