"use client";

import { useState } from "react";
import { api } from "@/lib/api";
import { useFlowStore, type StepType } from "@/store/flow-store";

const STEP_TYPES: StepType[] = ["WebhookTrigger", "Mapping", "Script", "HttpRequest"];

export function FlowDesigner({ flowId }: { flowId: string }) {
  const { steps, addStep, moveStep, updateStep } = useFlowStore();
  const [saving, setSaving] = useState(false);

  async function save() {
    setSaving(true);
    await api(`/api/flows/${flowId}/steps`, {
      method: "PUT",
      body: JSON.stringify({
        steps: steps.map((s) => ({
          name: s.name,
          stepType:
            s.stepType === "WebhookTrigger"
              ? 1
              : s.stepType === "Mapping"
                ? 2
                : s.stepType === "Script"
                  ? 3
                  : 4,
          config: s.config
        }))
      })
    });
    setSaving(false);
  }

  return (
    <div className="space-y-4">
      <div className="flex gap-2">
        {STEP_TYPES.map((type) => (
          <button key={type} onClick={() => addStep(type)}>{`+ ${type}`}</button>
        ))}
      </div>
      <div className="space-y-3">
        {steps.map((step, index) => (
          <div key={step.id} className="border border-slate-700 rounded p-3 bg-slate-900">
            <div className="flex items-center gap-2 mb-2">
              <strong className="w-36">{step.stepType}</strong>
              <input value={step.name} onChange={(e) => updateStep(step.id, { name: e.target.value })} />
              <button disabled={index === 0} onClick={() => moveStep(index, index - 1)}>Up</button>
              <button disabled={index === steps.length - 1} onClick={() => moveStep(index, index + 1)}>Down</button>
            </div>
            <textarea
              className="w-full h-28"
              value={JSON.stringify(step.config, null, 2)}
              onChange={(e) => {
                try {
                  updateStep(step.id, { config: JSON.parse(e.target.value) });
                } catch {
                  // ignore incomplete JSON while typing
                }
              }}
            />
          </div>
        ))}
      </div>
      <button onClick={save} disabled={saving}>{saving ? "Saving..." : "Save Flow"}</button>
    </div>
  );
}
