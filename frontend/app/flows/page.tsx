"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Flow {
  id: string;
  name: string;
  description: string;
  webhookKey: string;
}

export default function FlowsPage() {
  const [flows, setFlows] = useState<Flow[]>([]);
  const [name, setName] = useState("");

  async function load() {
    const data = await api<Flow[]>("/api/flows");
    setFlows(data);
  }

  useEffect(() => {
    void load();
  }, []);

  async function createFlow(e: FormEvent) {
    e.preventDefault();
    await api("/api/flows", { method: "POST", body: JSON.stringify({ name, description: "" }) });
    setName("");
    await load();
  }

  return (
    <div className="space-y-4">
      <h1 className="text-3xl font-semibold">Flows</h1>
      <form onSubmit={createFlow} className="flex gap-2">
        <input value={name} onChange={(e) => setName(e.target.value)} placeholder="New flow name" />
        <button>Create</button>
      </form>
      <div className="space-y-2">
        {flows.map((flow) => (
          <div key={flow.id} className="border border-slate-700 rounded p-3 bg-slate-900">
            <div className="flex justify-between items-center">
              <div>
                <h2 className="font-medium">{flow.name}</h2>
                <p className="text-sm text-slate-400">Webhook: /webhooks/{flow.webhookKey}</p>
              </div>
              <Link href={`/flows/${flow.id}`} className="underline">Open Designer</Link>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
