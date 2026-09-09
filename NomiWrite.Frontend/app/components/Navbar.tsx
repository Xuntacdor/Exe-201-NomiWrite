"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Menu, PenLine, X } from "lucide-react";
import { getSession } from "@/lib/auth/session";

const navLinks = [
  { label: "How it works", href: "/#how-it-works" },
  { label: "Features", href: "/#features" },
  { label: "Pricing", href: "/#pricing" },
];

export default function Navbar() {
  const [scrolled, setScrolled] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);
  const [signedIn, setSignedIn] = useState(false);

  useEffect(() => {
    const authTimer = setTimeout(() => {
      setSignedIn(Boolean(getSession()?.accessToken));
    }, 0);

    const onScroll = () => setScrolled(window.scrollY > 20);
    window.addEventListener("scroll", onScroll);
    return () => {
      clearTimeout(authTimer);
      window.removeEventListener("scroll", onScroll);
    };
  }, []);

  return (
    <header
      className={`fixed left-0 right-0 top-0 z-50 transition-all duration-300 ${
        scrolled ? "border-b border-slate-100 bg-white/90 shadow-sm backdrop-blur-md" : "bg-transparent"
      }`}
    >
      <nav className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <Link href={signedIn ? "/dashboard" : "/"} className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600">
            <PenLine className="h-4 w-4 text-white" strokeWidth={2.5} />
          </div>
          <span className="text-lg font-bold text-slate-900">NomiWrite</span>
        </Link>

        <div className="hidden items-center gap-8 md:flex">
          {navLinks.map(link => (
            <Link key={link.href} href={link.href} className="text-sm font-medium text-slate-600 transition-colors hover:text-blue-600">
              {link.label}
            </Link>
          ))}
        </div>

        <div className="hidden items-center gap-3 md:flex">
          <Link
            href={signedIn ? "/dashboard" : "/login"}
            className="text-sm font-medium text-slate-600 transition-colors hover:text-slate-900"
          >
            {signedIn ? "Dashboard" : "Login"}
          </Link>
          <Link
            href={signedIn ? "/write" : "/register"}
            className="rounded-full bg-blue-600 px-4 py-2 text-sm font-semibold text-white shadow-sm shadow-blue-200 transition-colors hover:bg-blue-700"
          >
            {signedIn ? "Write" : "Start free"}
          </Link>
        </div>

        <button
          className="rounded-lg p-2 text-slate-600 transition-colors hover:bg-slate-100 md:hidden"
          onClick={() => setMobileOpen(value => !value)}
          aria-label="Toggle menu"
          type="button"
        >
          {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
        </button>
      </nav>

      {mobileOpen && (
        <div className="space-y-3 border-t border-slate-100 bg-white px-4 py-4 md:hidden">
          {navLinks.map(link => (
            <Link
              key={link.href}
              href={link.href}
              className="block py-2 text-sm font-medium text-slate-700 hover:text-blue-600"
              onClick={() => setMobileOpen(false)}
            >
              {link.label}
            </Link>
          ))}
          <div className="flex flex-col gap-2 pt-2">
            <Link
              href={signedIn ? "/dashboard" : "/login"}
              className="block py-2 text-center text-sm font-medium text-slate-600"
              onClick={() => setMobileOpen(false)}
            >
              {signedIn ? "Dashboard" : "Login"}
            </Link>
            <Link
              href={signedIn ? "/write" : "/register"}
              className="block rounded-full bg-blue-600 py-2.5 text-center text-sm font-semibold text-white transition-colors hover:bg-blue-700"
              onClick={() => setMobileOpen(false)}
            >
              {signedIn ? "Write" : "Start free"}
            </Link>
          </div>
        </div>
      )}
    </header>
  );
}
