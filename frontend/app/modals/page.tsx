"use client";

import ModalDemo from "@/components/ui/modal-demo";

export default function ModalsPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto py-8">
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            macOS-Style Modal Animations
          </h1>
          <p className="text-lg text-gray-600 max-w-2xl mx-auto">
            Experience smooth modal animations inspired by macOS. Each modal features 
            zoom-in and zoom-out effects with backdrop blur for a premium feel.
          </p>
        </div>
        
        <div className="bg-white rounded-xl shadow-lg p-8">
          <ModalDemo />
        </div>
        
        <div className="mt-8 text-center">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-4xl mx-auto">
            <div className="bg-white rounded-lg p-6 shadow-md">
              <h3 className="font-semibold text-lg mb-2">Smooth Animations</h3>
              <p className="text-gray-600 text-sm">
                All modals feature macOS-inspired zoom and slide animations
              </p>
            </div>
            <div className="bg-white rounded-lg p-6 shadow-md">
              <h3 className="font-semibold text-lg mb-2">Blurred Backdrop</h3>
              <p className="text-gray-600 text-sm">
                Background content remains visible with elegant blur effects
              </p>
            </div>
            <div className="bg-white rounded-lg p-6 shadow-md">
              <h3 className="font-semibold text-lg mb-2">Responsive Design</h3>
              <p className="text-gray-600 text-sm">
                Modals adapt perfectly to different screen sizes
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
