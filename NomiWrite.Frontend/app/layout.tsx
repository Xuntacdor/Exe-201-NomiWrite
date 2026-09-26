import type { Metadata } from "next";
import { Nunito } from "next/font/google";
import "./globals.css";
import { themeScript } from "@/lib/theme-script";

const nunito = Nunito({
  subsets: ["latin", "vietnamese"],
  variable: "--font-nunito",
  display: "swap",
});

export const metadata: Metadata = {
  title: "NomiWrite - English writing practice",
  description:
    "Personalized English writing practice with AI-style feedback, grammar insights, vocabulary suggestions, quizzes, and progress tracking.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" className={`${nunito.variable} h-full antialiased`} suppressHydrationWarning>
      <head><script dangerouslySetInnerHTML={{ __html: themeScript }} /></head>
      <body className="min-h-full flex flex-col">{children}</body>
    </html>
  );
}
