"use client";

import { useState } from "react";
import { 
  Dialog, 
  DialogTrigger, 
  DialogContent, 
  DialogHeader, 
  DialogTitle, 
  DialogDescription,
  DialogFooter,
  DialogClose
} from "./dialog";
import { LoadingModal } from "./loading-modal";
import { LoadingOverlay } from "../loader/loading-overlay";
import { Button } from "./button";

export function ModalDemo() {
  const [isLoadingModalOpen, setIsLoadingModalOpen] = useState(false);
  const [isLoadingOverlayOpen, setIsLoadingOverlayOpen] = useState(false);

  const handleLoadingModal = () => {
    setIsLoadingModalOpen(true);
    // Auto close after 3 seconds for demo
    setTimeout(() => setIsLoadingModalOpen(false), 3000);
  };

  const handleLoadingOverlay = () => {
    setIsLoadingOverlayOpen(true);
    // Auto close after 3 seconds for demo
    setTimeout(() => setIsLoadingOverlayOpen(false), 3000);
  };

  return (
    <div className="flex flex-wrap gap-4 p-8">
      <h2 className="w-full text-2xl font-bold mb-6">Modal Animations Demo</h2>
      
      {/* Regular Dialog */}
      <Dialog>
        <DialogTrigger asChild>
          <Button variant="outline" className="transform transition-all duration-200 hover:scale-105 active:scale-95">
            Open Dialog Modal
          </Button>
        </DialogTrigger>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>macOS-Style Modal</DialogTitle>
            <DialogDescription>
              This modal features smooth zoom-in and zoom-out animations similar to macOS apps.
            </DialogDescription>
          </DialogHeader>
          <div className="flex items-center space-x-2">
            <div className="grid flex-1 gap-2">
              <p className="text-sm text-muted-foreground">
                Notice how the modal scales in smoothly and will scale out when closed.
              </p>
            </div>
          </div>
          <DialogFooter className="sm:justify-start">
            <DialogClose asChild>
              <Button type="button" variant="secondary">
                Close
              </Button>
            </DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Form Dialog */}
      <Dialog>
        <DialogTrigger asChild>
          <Button className="transform transition-all duration-200 hover:scale-105 active:scale-95">
            Open Form Modal
          </Button>
        </DialogTrigger>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Sample Form</DialogTitle>
            <DialogDescription>
              Enter your information below.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <label htmlFor="name" className="text-right text-sm font-medium">
                Name
              </label>
              <input
                id="name"
                className="col-span-3 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Enter your name"
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <label htmlFor="email" className="text-right text-sm font-medium">
                Email
              </label>
              <input
                id="email"
                type="email"
                className="col-span-3 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Enter your email"
              />
            </div>
          </div>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </DialogClose>
            <Button type="submit">Save</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Loading Modal */}
      <Button 
        variant="outline" 
        onClick={handleLoadingModal}
        className="transform transition-all duration-200 hover:scale-105 active:scale-95"
      >
        Show Loading Modal
      </Button>

      {/* Loading Overlay */}
      <Button 
        variant="outline" 
        onClick={handleLoadingOverlay}
        className="transform transition-all duration-200 hover:scale-105 active:scale-95"
      >
        Show Loading Overlay
      </Button>

      {/* Large Content Dialog */}
      <Dialog>
        <DialogTrigger asChild>
          <Button variant="outline" className="transform transition-all duration-200 hover:scale-105 active:scale-95">
            Large Content Modal
          </Button>
        </DialogTrigger>
        <DialogContent className="sm:max-w-2xl">
          <DialogHeader>
            <DialogTitle>Large Modal with Animation</DialogTitle>
            <DialogDescription>
              This modal contains more content and still maintains smooth animations.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="h-32 bg-gray-100 rounded-lg flex items-center justify-center">
              <p className="text-gray-600">Content Area 1</p>
            </div>
            <div className="h-32 bg-gray-100 rounded-lg flex items-center justify-center">
              <p className="text-gray-600">Content Area 2</p>
            </div>
            <div className="h-32 bg-gray-100 rounded-lg flex items-center justify-center">
              <p className="text-gray-600">Content Area 3</p>
            </div>
          </div>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="secondary">
                Close
              </Button>
            </DialogClose>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Confirmation Dialog */}
      <Dialog>
        <DialogTrigger asChild>
          <Button variant="destructive" className="transform transition-all duration-200 hover:scale-105 active:scale-95">
            Delete Action
          </Button>
        </DialogTrigger>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>Are you sure?</DialogTitle>
            <DialogDescription>
              This action cannot be undone. This will permanently delete your data.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="flex gap-2">
            <DialogClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </DialogClose>
            <Button type="button" variant="destructive">
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Loading Modal Component */}
      <LoadingModal
        isOpen={isLoadingModalOpen}
        title="Processing..."
        message="Please wait while we process your request"
        onClose={() => setIsLoadingModalOpen(false)}
      />

      {/* Loading Overlay Component */}
      <LoadingOverlay
        isOpen={isLoadingOverlayOpen}
        title="Loading..."
        message="Fetching your data"
        onClose={() => setIsLoadingOverlayOpen(false)}
      />
    </div>
  );
}

export default ModalDemo;
