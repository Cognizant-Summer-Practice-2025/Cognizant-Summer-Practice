export function PublishSidebar() {
  return (
    <div className="flex flex-col gap-4 md:gap-6">
      {/* Publication Status */}
      <div className="bg-white rounded-lg border border-slate-200 p-4 md:p-6">
        <div className="pb-1 flex flex-col mb-4">
          <h3 className="text-slate-900 text-lg font-semibold">Publication Status</h3>
        </div>
        
        <div className="flex items-center gap-2 mb-4">
          <div className="pb-1 flex flex-col">
            <span className="text-slate-900 text-base">🌐</span>
          </div>
          <div className="pb-1 flex flex-col">
            <span className="text-blue-600 text-base font-medium">Public</span>
          </div>
        </div>
        
        <div className="flex flex-col">
          <p className="text-slate-500 text-sm">Last updated: 15/03/2024</p>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="bg-white rounded-lg border border-slate-200 p-4 md:p-6">
        <div className="pb-1 flex flex-col mb-4">
          <h3 className="text-slate-900 text-lg font-semibold">Quick Stats</h3>
        </div>
        
        <div className="space-y-2">
          <div className="py-2 border-b border-slate-200 flex justify-between items-center">
            <div className="flex flex-col">
              <span className="text-slate-500 text-sm">Projects:</span>
            </div>
            <div className="flex flex-col">
              <span className="text-slate-900 text-sm font-semibold">2</span>
            </div>
          </div>
          
          <div className="py-2 border-b border-slate-200 flex justify-between items-center">
            <div className="flex flex-col">
              <span className="text-slate-500 text-sm">Blog Posts:</span>
            </div>
            <div className="flex flex-col">
              <span className="text-slate-900 text-sm font-semibold">1</span>
            </div>
          </div>
          
          <div className="py-2 flex justify-between items-center">
            <div className="flex flex-col">
              <span className="text-slate-500 text-sm">Skills:</span>
            </div>
            <div className="flex flex-col">
              <span className="text-slate-900 text-sm font-semibold">4</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
} 