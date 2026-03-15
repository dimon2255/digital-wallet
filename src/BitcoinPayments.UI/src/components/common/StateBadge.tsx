const stateColors: Record<string, string> = {
  settled: 'bg-green-500/15 text-green-400',
  transfercompleted: 'bg-green-500/15 text-green-400',
  confirming: 'bg-blue-500/15 text-blue-400',
  mempool: 'bg-amber-500/15 text-amber-400',
  broadcasting: 'bg-amber-500/15 text-amber-400',
  created: 'bg-zinc-500/15 text-zinc-400',
  failed: 'bg-red-500/15 text-red-400',
  voided: 'bg-zinc-500/15 text-zinc-400',
  expired: 'bg-zinc-500/15 text-zinc-400',
  authactive: 'bg-blue-500/15 text-blue-400',
};

export default function StateBadge({ state }: { state: string }) {
  const color = stateColors[state] || 'bg-zinc-500/15 text-zinc-400';
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${color}`}>
      {state}
    </span>
  );
}
