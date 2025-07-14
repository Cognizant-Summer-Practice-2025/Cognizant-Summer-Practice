import { Button } from "@/components/ui/button"

export function PublishHeader() {
  return (
    <div className="w-full px-4 py-3 md:px-6 md:py-4 lg:px-8 bg-white border-b border-slate-200 flex justify-between items-center">
      <div className="flex flex-col justify-start items-start">
        <button className="px-2 py-2 flex justify-center items-center hover:bg-slate-50 rounded-md transition-colors">
          <span className="text-slate-900 text-sm font-normal">← Back</span>
        </button>
      </div>
      
      <div className="flex justify-start items-center gap-1 sm:gap-2 md:gap-3">
        <Button 
          variant="outline" 
          className="px-2 py-2 sm:px-3 md:px-4 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 text-xs sm:text-sm"
        >
          <span className="text-slate-900 font-normal">
            <span className="hidden sm:inline">👁️ Preview</span>
            <span className="sm:hidden">👁️</span>
          </span>
        </Button>
        
        <Button 
          variant="outline" 
          className="px-2 py-2 sm:px-3 md:px-4 rounded-lg border border-slate-200 bg-slate-100 hover:bg-slate-200 text-xs sm:text-sm"
        >
          <span className="text-slate-900 font-normal">
            <span className="hidden sm:inline">💾 Save Draft</span>
            <span className="sm:hidden">💾</span>
          </span>
        </Button>
        
        <Button className="px-2 py-2 sm:px-3 md:px-4 rounded-lg bg-slate-900 hover:bg-slate-800 text-white text-xs sm:text-sm">
          <span className="text-white font-normal">
            <span className="hidden sm:inline">🌐 Publish</span>
            <span className="sm:hidden">🌐</span>
          </span>
        </Button>
      </div>
    </div>
  )
} 