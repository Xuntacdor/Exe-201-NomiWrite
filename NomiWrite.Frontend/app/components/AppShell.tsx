import AppSidebar from "./AppSidebar";

interface AppShellProps {
  children: React.ReactNode;
  activePath?: string;
}

export default function AppShell({ children, activePath }: AppShellProps) {
  return (
    <div className="app-shell flex bg-canvas">
      <AppSidebar activePath={activePath} />
      <main className="app-main flex-1">
        {children}
      </main>
    </div>
  );
}
