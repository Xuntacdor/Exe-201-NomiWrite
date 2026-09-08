import Link from "next/link";
import { MapPin, PenLine } from "lucide-react";

const footerLinks = {
  Product: [
    { label: "How it works", href: "/#how-it-works" },
    { label: "Features", href: "/#features" },
    { label: "Pricing", href: "/#pricing" },
    { label: "Writing guide", href: "/guide" },
  ],
  Support: [
    { label: "Dashboard", href: "/dashboard" },
    { label: "Profile", href: "/profile" },
    { label: "History", href: "/history" },
    { label: "Upgrade", href: "/upgrade" },
  ],
  "Writing Types": [
    { label: "IELTS Writing Task 2", href: "/write" },
    { label: "IELTS Writing Task 1", href: "/write" },
    { label: "Business Email", href: "/write" },
    { label: "Cover Letter", href: "/write" },
  ],
};

export default function Footer() {
  return (
    <footer className="border-t border-slate-800 bg-slate-900">
      <div className="mx-auto max-w-6xl px-4 py-14 sm:px-6">
        <div className="grid grid-cols-1 gap-10 md:grid-cols-4">
          <div className="md:col-span-1">
            <Link href="/" className="mb-4 flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600">
                <PenLine className="h-4 w-4 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-base font-bold text-white">NomiWrite</span>
            </Link>
            <p className="text-sm leading-relaxed text-slate-400">
              English writing practice with AI feedback, saved submissions, and a backend-first learning loop.
            </p>
            <p className="mt-5 flex items-center gap-1.5 text-xs text-slate-600">
              <MapPin className="h-3 w-3 text-slate-500" />
              Made in Vietnam
            </p>
          </div>

          {Object.entries(footerLinks).map(([category, links]) => (
            <div key={category}>
              <h4 className="mb-4 text-xs font-semibold uppercase tracking-wider text-slate-400">
                {category}
              </h4>
              <ul className="space-y-2.5">
                {links.map(link => (
                  <li key={link.label}>
                    <Link href={link.href} className="text-sm text-slate-500 transition-colors hover:text-slate-300">
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="mt-12 flex flex-col items-center justify-between gap-3 border-t border-slate-800 pt-6 sm:flex-row">
          <p className="text-xs text-slate-600">
            Copyright 2026 NomiWrite.
          </p>
          <p className="text-xs text-slate-600">
            AI scores are learning guidance, not official exam results.
          </p>
        </div>
      </div>
    </footer>
  );
}
