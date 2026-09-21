import { BrowserRouter, Routes, Route, Outlet } from "react-router-dom";
import { Navigate, useLocation } from "react-router-dom";
import AdminSidebar from "./components/AdminSidebar";
import Dashboard from "./pages/Dashboard";
import Users from "./pages/Users";
import Prompts from "./pages/Prompts";
import Submissions from "./pages/Submissions";
import Login from "./pages/Login";
import { hasAdminSession, importSessionFromHash } from "./lib/authSession";

function RequireAdmin() {
  const location = useLocation();
  importSessionFromHash();

  if (!hasAdminSession()) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}

// Layout component wrapping the sidebar and content area
function AdminLayout() {
  return (
    <div className="flex h-screen w-full bg-slate-50 font-sans antialiased text-slate-900">
      <AdminSidebar />
      <main className="flex flex-1 flex-col overflow-hidden">
        <div className="flex-1 overflow-y-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route element={<RequireAdmin />}>
          <Route path="/" element={<AdminLayout />}>
            <Route index element={<Dashboard />} />
            <Route path="users" element={<Users />} />
            <Route path="prompts" element={<Prompts />} />
            <Route path="submissions" element={<Submissions />} />
            <Route path="settings" element={<div className="p-8 text-slate-500 font-bold">Settings Placeholder</div>} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
