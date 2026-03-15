const stateConfig: Record<string, { bg: string; text: string; dot: string }> = {
  settled:           { bg: 'bg-green-500/10', text: 'text-green-400', dot: 'bg-green-400' },
  transfercompleted: { bg: 'bg-green-500/10', text: 'text-green-400', dot: 'bg-green-400' },
  confirming:        { bg: 'bg-blue-500/10',  text: 'text-blue-400',  dot: 'bg-blue-400' },
  mempool:           { bg: 'bg-amber-500/10', text: 'text-amber-400', dot: 'bg-amber-400' },
  broadcasting:      { bg: 'bg-amber-500/10', text: 'text-amber-400', dot: 'bg-amber-400' },
  created:           { bg: 'bg-zinc-500/10',  text: 'text-zinc-400',  dot: 'bg-zinc-400' },
  failed:            { bg: 'bg-red-500/10',   text: 'text-red-400',   dot: 'bg-red-400' },
  voided:            { bg: 'bg-zinc-500/10',  text: 'text-zinc-400',  dot: 'bg-zinc-500' },
  expired:           { bg: 'bg-zinc-500/10',  text: 'text-zinc-400',  dot: 'bg-zinc-500' },
  authactive:        { bg: 'bg-blue-500/10',  text: 'text-blue-400',  dot: 'bg-blue-400' },
};

const fallback = { bg: 'bg-zinc-500/10', text: 'text-zinc-400', dot: 'bg-zinc-500' };

export default function StateBadge({ state }: { state: string }) {
  const { bg, text, dot } = stateConfig[state] || fallback;
  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-medium tracking-wide ${bg} ${text}`}
      style={{ fontFamily: 'var(--font-body)' }}
    >
      <span className={`w-1.5 h-1.5 rounded-full ${dot}`} />
      {state}
    </span>
  );
}
