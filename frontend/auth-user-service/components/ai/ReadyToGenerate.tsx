import React from 'react';
import { Brain } from 'lucide-react';

export function ReadyToGenerate() {
  return (
    <div className="py-12 text-center">
      <div className="p-4 bg-gray-100 rounded-full w-16 h-16 mb-4 flex items-center justify-center mx-auto">
        <Brain className="w-8 h-8 text-gray-400" />
      </div>
      <h3 className="text-lg font-medium text-gray-900 mb-2">Ready to Generate</h3>
      <p className="text-gray-600 max-w-md mx-auto">
        Click the generate button above to let our AI curate the best portfolios for you.
      </p>
    </div>
  );
}

