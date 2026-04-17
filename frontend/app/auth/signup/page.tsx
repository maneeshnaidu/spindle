"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

export default function SignupPage() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const router = useRouter();

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    try {
      const result = await api<{ token: string }>("/api/auth/signup", {
        method: "POST",
        body: JSON.stringify({ email, password })
      });
      localStorage.setItem("token", result.token);
      router.push("/flows");
    } catch (err) {
      setError(String(err));
    }
  }

  return (
    <form onSubmit={onSubmit} className="max-w-md space-y-3">
      <h1 className="text-2xl font-semibold">Sign up</h1>
      <input className="w-full" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
      <input className="w-full" type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
      {error && <p className="text-red-400">{error}</p>}
      <button type="submit">Create account</button>
    </form>
  );
}
