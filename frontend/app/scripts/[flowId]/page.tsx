"use client";

import { useState } from "react";
import { api } from "@/lib/api";

export default function ScriptsPage() {
  const [prompt, setPrompt] = useState("Normalize the message and return only id, email in lowercase.");
  const [sourceCode, setSourceCode] = useState("log('running'); return message;");

  async function generateWithAi() {
    const result = await api<{ result: string }>("/api/ai/script-generation", {
      method: "POST",
      body: JSON.stringify({ prompt })
    });
    setSourceCode(result.result);
  }

  return (
    <div className="space-y-4">
      <h1 className="text-3xl font-semibold">JavaScript Script Editor</h1>
      <textarea className="w-full h-24" value={prompt} onChange={(e) => setPrompt(e.target.value)} />
      <button onClick={generateWithAi}>AI Generate Script</button>
      <textarea className="w-full h-80 font-mono" value={sourceCode} onChange={(e) => setSourceCode(e.target.value)} />
      <p className="text-sm text-slate-400">Scripts can use message, log(string), variables and must return JSON.</p>
    </div>
  );
}
