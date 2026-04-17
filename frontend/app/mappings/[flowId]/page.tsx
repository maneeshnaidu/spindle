"use client";

import { useState } from "react";
import { api } from "@/lib/api";

export default function MappingsPage() {
  const [sourceJson, setSourceJson] = useState('{"customer":{"name":"Jane","email":"jane@acme.com"}}');
  const [targetJson, setTargetJson] = useState('{"fullName":"","emailAddress":""}');
  const [rules, setRules] = useState('[{"targetPath":"fullName","sourcePath":"customer.name"},{"targetPath":"emailAddress","sourcePath":"customer.email"}]');
  const [aiResult, setAiResult] = useState("");

  async function askAi() {
    const result = await api<{ result: string }>("/api/ai/mapping-suggestions", {
      method: "POST",
      body: JSON.stringify({ sourceJson, targetJson })
    });
    setAiResult(result.result);
  }

  async function testMapping() {
    const parsedRules = JSON.parse(rules);
    const mapped: Record<string, unknown> = {};
    for (const rule of parsedRules) {
      const sourcePath = (rule.sourcePath ?? "") as string;
      const value = sourcePath.split(".").reduce((acc: any, key: string) => acc?.[key], JSON.parse(sourceJson));
      mapped[rule.targetPath] = value;
    }
    alert(JSON.stringify(mapped, null, 2));
  }

  return (
    <div className="space-y-4">
      <h1 className="text-3xl font-semibold">JSON Mapping</h1>
      <div className="grid grid-cols-2 gap-4">
        <textarea className="h-56" value={sourceJson} onChange={(e) => setSourceJson(e.target.value)} />
        <textarea className="h-56" value={targetJson} onChange={(e) => setTargetJson(e.target.value)} />
      </div>
      <textarea className="w-full h-56" value={rules} onChange={(e) => setRules(e.target.value)} />
      <div className="flex gap-2">
        <button onClick={testMapping}>Test Mapping</button>
        <button onClick={askAi}>AI Suggest Mapping</button>
      </div>
      {aiResult && <pre className="bg-slate-900 border border-slate-700 p-3 rounded whitespace-pre-wrap">{aiResult}</pre>}
    </div>
  );
}
