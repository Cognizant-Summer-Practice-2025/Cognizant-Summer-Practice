interface SkillTagProps {
  skill: string
  onRemove: (skill: string) => void
}

export function SkillTag({ skill, onRemove }: SkillTagProps) {
  return (
    <div className="relative bg-slate-100 rounded-md px-3 py-1 flex items-center gap-2">
      <span className="text-slate-900 text-sm font-medium">{skill}</span>
      <button
        onClick={() => onRemove(skill)}
        className="text-slate-500 hover:text-slate-700 text-xs"
      >
        ✕
      </button>
    </div>
  )
} 