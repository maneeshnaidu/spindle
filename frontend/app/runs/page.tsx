"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Run {
  id: string;
  flowId: string;
  status: string;
  startedAtUtc: string;
  completedAtUtc?: string;
  error?: string;
}

export default function RunsPage() {
  const [runs, setRuns] = useState<Run[]>([]);
  useEffect(() => {
    api<Run[]>("/api/runs").then(setRuns).catch(() => undefined);
  }, []);

  return (
    <div className="space-y-4">
      <h1 className="text-3xl font-semibold">Execution Runs</h1>
      {runs.map((run) => (
        <div key={run.id} className="border border-slate-700 rounded p-3 bg-slate-900">
          <div className="flex justify-between">
            <span>{run.status}</span>
            <Link href={`/runs/${run.id}`} className="underline">View Logs</Link>
          </div>
          <p className="text-sm text-slate-400">{run.startedAtUtc}</p>
          {run.error && <p className="text-red-400 text-sm">{run.error}</p>}
        </div>
      ))}
    </div>
  );
}
