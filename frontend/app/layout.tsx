import "./globals.css";
import Link from "next/link";
import type { ReactNode } from "react";

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>
        <div className="min-h-screen">
          <nav className="border-b border-slate-800 px-6 py-4 flex gap-4">
            <Link href="/flows">Flows</Link>
            <Link href="/mappings/sample">Mappings</Link>
            <Link href="/scripts/sample">Scripts</Link>
            <Link href="/runs">Runs</Link>
            <Link href="/auth/login">Login</Link>
          </nav>
          <main className="p-6">{children}</main>
        </div>
      </body>
    </html>
  );
}
