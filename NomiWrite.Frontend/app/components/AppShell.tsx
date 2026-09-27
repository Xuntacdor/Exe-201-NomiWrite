"use client";

import { useEffect, useRef } from "react";
import AppSidebar from "./AppSidebar";

interface AppShellProps {
  children: React.ReactNode;
  activePath?: string;
  writingWorkspace?: boolean;
}

export default function AppShell({ children, activePath, writingWorkspace = false }: AppShellProps) {
  const shellRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const shell = shellRef.current;
    const header = shell?.querySelector<HTMLElement>(".app-main > .sticky");
    if (!shell || !header) return;

    // Include wrapped actions and translated titles in the shared header height.
    const syncHeight = () => {
      shell.style.setProperty("--shell-header-height", `${header.getBoundingClientRect().height}px`);
    };
    syncHeight();
    const observer = new ResizeObserver(syncHeight);
    observer.observe(header);
    return () => observer.disconnect();
  }, [activePath, children]);

  return (
    <div ref={shellRef} className="app-shell flex bg-canvas">
      <AppSidebar activePath={activePath} />
      <main className={`app-main flex-1${writingWorkspace ? " app-main--writing" : ""}`}>
        {children}
      </main>
    </div>
  );
}
