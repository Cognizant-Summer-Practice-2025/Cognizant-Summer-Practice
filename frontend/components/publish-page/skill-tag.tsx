import { Skill } from '@/lib/interfaces';

interface SkillTagProps {
  skill: Skill;
  onRemove: (skillId: string) => void;
  onUpdateProficiency?: (skillId: string, level: number) => void;
  onUpdateCategory?: (skillId: string, category: string) => void;
  showDetails?: boolean;
}

export function SkillTag({ skill, onRemove, onUpdateProficiency, onUpdateCategory, showDetails = false }: SkillTagProps) {
  if (showDetails) {
    return (
      <div className="relative bg-white border border-slate-200 rounded-lg p-3 flex flex-col gap-2">
        <div className="flex items-center justify-between">
          <div>
            <span className="text-slate-900 text-sm font-medium">{skill.name}</span>
            <div className="text-xs text-slate-500">{skill.category}</div>
          </div>
          <button
            onClick={() => onRemove(skill.id)}
            className="text-slate-500 hover:text-slate-700 text-xs"
          >
            ✕
          </button>
        </div>
        
        {onUpdateProficiency && (
          <div className="flex items-center gap-2">
            <span className="text-xs text-slate-600 min-w-[80px]">
              {skill.proficiencyLevel}%
            </span>
            <input
              type="range"
              min="1"
              max="100"
              value={skill.proficiencyLevel}
              onChange={(e) => onUpdateProficiency(skill.id, Number(e.target.value))}
              className="flex-1 h-1 bg-slate-200 rounded-lg appearance-none cursor-pointer"
            />
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="relative bg-slate-100 rounded-md px-3 py-1 flex items-center gap-2">
      <span className="text-slate-900 text-sm font-medium">{skill.name}</span>
      <span className="text-slate-500 text-xs">({skill.category})</span>
      <button
        onClick={() => onRemove(skill.id)}
        className="text-slate-500 hover:text-slate-700 text-xs"
      >
        ✕
      </button>
    </div>
  );
} 