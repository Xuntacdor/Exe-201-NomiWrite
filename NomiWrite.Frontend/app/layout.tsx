import type { Metadata } from "next";
import { Nunito } from "next/font/google";
import "./globals.css";
import { themeScript } from "@/lib/theme-script";
import { LocaleProvider } from "@/lib/i18n/locale";
import PublicSettings from "./components/PublicSettings";

const nunito = Nunito({
  subsets: ["latin", "vietnamese"],
  variable: "--font-nunito",
  display: "swap",
});

export const metadata: Metadata = {
  title: "NomiWrite - Luyện viết tiếng Anh",
  description:
    "Personalized English writing practice with AI-style feedback, grammar insights, vocabulary suggestions, quizzes, and progress tracking.",
  icons: {
    icon: [{ url: "/nomiwrite-mark.svg", type: "image/svg+xml" }],
    shortcut: "/nomiwrite-mark.svg",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" className={`${nunito.variable} h-full antialiased`} suppressHydrationWarning>
      <head><script dangerouslySetInnerHTML={{ __html: themeScript }} /></head>
      <body className="min-h-full flex flex-col"><LocaleProvider>{children}<PublicSettings /></LocaleProvider></body>
    </html>
  );
}
