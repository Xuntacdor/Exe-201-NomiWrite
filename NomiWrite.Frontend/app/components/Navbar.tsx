"use client";

import { useLocale } from "@/lib/i18n/locale";


import { useEffect, useState } from "react";
import Link from "next/link";
import { Menu, X } from "lucide-react";
import BrandMark from "./BrandMark";

const navLinks = [
  { label: "How it works", href: "/#how-it-works" },
  { label: "Features", href: "/#features" },
  { label: "Pricing", href: "/#pricing" },
];

export default function Navbar() {
  const { t: translateUi } = useLocale();
  const [scrolled, setScrolled] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 20);
    window.addEventListener("scroll", onScroll);
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <header
      className={`fixed left-0 right-0 top-0 z-50 transition-all duration-300 ${
        scrolled ? "border-b border-line bg-surface shadow-sm " : "bg-transparent"
      }`}
    >
      <nav className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <Link href="/" className="flex items-center gap-2">
          <BrandMark size={32} />
          <span className="text-lg font-bold text-ink">{translateUi("NomiWrite")}</span>
        </Link>

        <div className="hidden items-center gap-8 md:flex">
          {navLinks.map(link => (
            <Link key={link.href} href={link.href} className="text-sm font-medium text-muted transition-colors hover:text-accent-ink">
              {translateUi(link.label)}
            </Link>
          ))}
        </div>

        <div className="hidden items-center gap-3 md:flex">
          <Link
            href="/login"
            className="text-sm font-medium text-muted transition-colors hover:text-ink"
          >
            {translateUi("Login")}</Link>
          <Link
            href="/register"
            className="rounded-full bg-accent px-4 py-2 text-sm font-semibold text-ink shadow-sm transition-colors hover:bg-accent-hover"
          >
            {translateUi("Start free")}</Link>
        </div>

        <button
          className="rounded-lg p-2 text-muted transition-colors hover:bg-surface-muted md:hidden"
          onClick={() => setMobileOpen(value => !value)}
          aria-label={translateUi("Toggle menu")}
          type="button"
        >
          {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
        </button>
      </nav>

      {mobileOpen && (
        <div className="space-y-3 border-t border-line bg-surface px-4 py-4 md:hidden">
          {navLinks.map(link => (
            <Link
              key={link.href}
              href={link.href}
              className="block py-2 text-sm font-medium text-ink hover:text-accent-ink"
              onClick={() => setMobileOpen(false)}
            >
              {translateUi(link.label)}
            </Link>
          ))}
          <div className="flex flex-col gap-2 pt-2">
            <Link
              href="/login"
              className="block py-2 text-center text-sm font-medium text-muted"
              onClick={() => setMobileOpen(false)}
            >
              {translateUi("Login")}</Link>
            <Link
              href="/register"
              className="block rounded-full bg-accent py-2.5 text-center text-sm font-semibold text-ink transition-colors hover:bg-accent-hover"
              onClick={() => setMobileOpen(false)}
            >
              {translateUi("Start free")}</Link>
          </div>
        </div>
      )}
    </header>
  );
}
