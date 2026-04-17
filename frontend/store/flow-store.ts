"use client";

import { create } from "zustand";

export type StepType = "WebhookTrigger" | "Mapping" | "Script" | "HttpRequest";

export interface FlowStep {
  id: string;
  name: string;
  stepType: StepType;
  config: Record<string, unknown>;
}

interface FlowState {
  steps: FlowStep[];
  addStep: (stepType: StepType) => void;
  moveStep: (from: number, to: number) => void;
  updateStep: (id: string, patch: Partial<FlowStep>) => void;
}

export const useFlowStore = create<FlowState>((set) => ({
  steps: [],
  addStep: (stepType) =>
    set((state) => ({
      steps: [...state.steps, { id: crypto.randomUUID(), name: stepType, stepType, config: {} }]
    })),
  moveStep: (from, to) =>
    set((state) => {
      const steps = [...state.steps];
      const [item] = steps.splice(from, 1);
      steps.splice(to, 0, item);
      return { steps };
    }),
  updateStep: (id, patch) =>
    set((state) => ({
      steps: state.steps.map((s) => (s.id === id ? { ...s, ...patch } : s))
    }))
}));
