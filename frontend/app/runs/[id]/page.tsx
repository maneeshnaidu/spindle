"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface RunDetails {
  id: string;
  status: string;
  outputPayload?: unknown;
  error?: string;
  steps: Array<{ id: string; stepName: string; status: string; log?: string; outputPayload?: unknown }>;
}

export default function RunDetailsPage({ params }: { params: { id: string } }) {
  const [run, setRun] = useState<RunDetails | null>(null);
  useEffect(() => {
    api<RunDetails>(`/api/runs/${params.id}`).then(setRun).catch(() => undefined);
  }, [params.id]);
  if (!run) return <p>Loading...</p>;

  return (
    <div className="space-y-4">
      <h1 className="text-3xl font-semibold">Run {run.id}</h1>
      <p>Status: {run.status}</p>
      {run.error && <p className="text-red-400">{run.error}</p>}
      <div className="space-y-2">
        {run.steps.map((step) => (
          <div key={step.id} className="border border-slate-700 rounded p-3 bg-slate-900">
            <h2 className="font-medium">{step.stepName}</h2>
            <p className="text-sm">Status: {step.status}</p>
            {step.log && <pre className="text-xs whitespace-pre-wrap">{step.log}</pre>}
          </div>
        ))}
      </div>
      <pre className="bg-slate-900 border border-slate-700 p-3 rounded text-xs whitespace-pre-wrap">{JSON.stringify(run.outputPayload, null, 2)}</pre>
    </div>
  );
}
