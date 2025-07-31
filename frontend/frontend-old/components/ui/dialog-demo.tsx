"use client";

import { useState } from "react";
import { Button } from "./button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./dialog";

export function DialogDemo() {
  const [isOpen1, setIsOpen1] = useState(false);
  const [isOpen2, setIsOpen2] = useState(false);
  const [isOpen3, setIsOpen3] = useState(false);

  return (
    <div className="flex flex-col gap-4 p-8">
      <h2 className="text-2xl font-bold mb-4">macOS-Style Dialog Demo</h2>
      
      <div className="flex gap-4 flex-wrap">
        {/* Dialog 1 - Simple confirmation */}
        <Dialog open={isOpen1} onOpenChange={setIsOpen1}>
          <DialogTrigger asChild>
            <Button variant="outline" className="bg-blue-500 hover:bg-blue-600 text-white border-blue-500">
              Open Simple Dialog
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[425px]">
            <DialogHeader>
              <DialogTitle>Confirm Action</DialogTitle>
              <DialogDescription>
                Are you sure you want to perform this action? This cannot be undone.
              </DialogDescription>
            </DialogHeader>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsOpen1(false)}>
                Cancel
              </Button>
              <Button onClick={() => setIsOpen1(false)}>
                Confirm
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* Dialog 2 - Form dialog */}
        <Dialog open={isOpen2} onOpenChange={setIsOpen2}>
          <DialogTrigger asChild>
            <Button variant="outline" className="bg-green-500 hover:bg-green-600 text-white border-green-500">
              Open Form Dialog
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[500px]">
            <DialogHeader>
              <DialogTitle>Create New Item</DialogTitle>
              <DialogDescription>
                Fill in the details below to create a new item.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-4 items-center gap-4">
                <label htmlFor="name" className="text-right">
                  Name
                </label>
                <input
                  id="name"
                  placeholder="Enter name"
                  className="col-span-3 px-3 py-2 border rounded-md"
                />
              </div>
              <div className="grid grid-cols-4 items-center gap-4">
                <label htmlFor="description" className="text-right">
                  Description
                </label>
                <textarea
                  id="description"
                  placeholder="Enter description"
                  className="col-span-3 px-3 py-2 border rounded-md"
                  rows={3}
                />
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsOpen2(false)}>
                Cancel
              </Button>
              <Button onClick={() => setIsOpen2(false)}>
                Create
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        {/* Dialog 3 - Large content dialog */}
        <Dialog open={isOpen3} onOpenChange={setIsOpen3}>
          <DialogTrigger asChild>
            <Button variant="outline" className="bg-purple-500 hover:bg-purple-600 text-white border-purple-500">
              Open Large Dialog
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[600px] max-h-[80vh] overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Settings & Preferences</DialogTitle>
              <DialogDescription>
                Configure your application settings and preferences.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-6 py-4">
              <div className="space-y-4">
                <h3 className="text-lg font-medium">General Settings</h3>
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <label className="text-sm font-medium">Enable notifications</label>
                    <input type="checkbox" className="rounded" />
                  </div>
                  <div className="flex items-center justify-between">
                    <label className="text-sm font-medium">Auto-save changes</label>
                    <input type="checkbox" className="rounded" defaultChecked />
                  </div>
                  <div className="flex items-center justify-between">
                    <label className="text-sm font-medium">Dark mode</label>
                    <input type="checkbox" className="rounded" />
                  </div>
                </div>
              </div>
              
              <div className="space-y-4">
                <h3 className="text-lg font-medium">Privacy Settings</h3>
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <label className="text-sm font-medium">Share analytics data</label>
                    <input type="checkbox" className="rounded" />
                  </div>
                  <div className="flex items-center justify-between">
                    <label className="text-sm font-medium">Allow data collection</label>
                    <input type="checkbox" className="rounded" />
                  </div>
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="text-lg font-medium">Advanced Options</h3>
                <div className="space-y-3">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="text-sm font-medium">Cache size (MB)</label>
                      <input type="number" defaultValue="100" className="w-full px-3 py-2 border rounded-md mt-1" />
                    </div>
                    <div>
                      <label className="text-sm font-medium">Session timeout (minutes)</label>
                      <input type="number" defaultValue="30" className="w-full px-3 py-2 border rounded-md mt-1" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setIsOpen3(false)}>
                Reset to Defaults
              </Button>
              <Button variant="outline" onClick={() => setIsOpen3(false)}>
                Cancel
              </Button>
              <Button onClick={() => setIsOpen3(false)}>
                Save Changes
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <div className="mt-8 p-4 bg-gray-50 rounded-lg">
        <h3 className="text-lg font-semibold mb-2">macOS-Style Features:</h3>
        <ul className="list-disc list-inside space-y-1 text-sm">
          <li>Smooth zoom-in/zoom-out animations (95% to 100% scale)</li>
          <li>Subtle slide-in from top animation</li>
          <li>Backdrop blur effect</li>
          <li>Longer animation duration (300ms) for smooth feel</li>
          <li>Fade-in/fade-out overlay transitions</li>
          <li>Click outside to close functionality</li>
        </ul>
      </div>
    </div>
  );
}
