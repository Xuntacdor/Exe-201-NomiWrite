import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";

const inter = Inter({ subsets: ["latin"] });

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
    <html lang="vi" className={`h-full antialiased ${inter.className}`}>
      <body className="min-h-full flex flex-col">{children}</body>
    </html>
  );
}
