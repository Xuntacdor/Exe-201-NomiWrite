import AppSidebar from "./AppSidebar";

interface AppShellProps {
  children: React.ReactNode;
  activePath?: string;
}

export default function AppShell({ children, activePath }: AppShellProps) {
  return (
    <div className="flex h-screen overflow-hidden bg-slate-50">
      <AppSidebar activePath={activePath} />
      <main className="flex-1 overflow-y-auto">
        {children}
      </main>
    </div>
  );
}
