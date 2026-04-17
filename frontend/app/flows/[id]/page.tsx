import Link from "next/link";
import { FlowDesigner } from "@/components/flow-designer";

export default function FlowDesignerPage({ params }: { params: { id: string } }) {
  return (
    <div className="space-y-4">
      <h1 className="text-3xl font-semibold">Flow Designer</h1>
      <div className="flex gap-4 text-sm">
        <Link href={`/mappings/${params.id}`} className="underline">Mappings</Link>
        <Link href={`/scripts/${params.id}`} className="underline">Scripts</Link>
      </div>
      <FlowDesigner flowId={params.id} />
    </div>
  );
}
