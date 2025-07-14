"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"

export function AddBlogPost() {
  const [blogData, setBlogData] = useState({
    title: "",
    excerpt: "",
    content: "",
    readTime: ""
  })

  const handleInputChange = (field: string, value: string) => {
    setBlogData(prev => ({ ...prev, [field]: value }))
  }

  const handleAddBlogPost = () => {
    // Handle blog post submission
    console.log("Adding blog post:", blogData)
  }

  return (
    <div className="w-full flex flex-col gap-4 md:gap-6">
      <div className="flex flex-col">
        <h2 className="text-slate-900 text-xl md:text-2xl font-semibold">Add New Blog Post</h2>
      </div>
      
      <div className="px-4 py-4 md:px-6 md:py-6 bg-white rounded-lg border border-slate-200 flex flex-col gap-4 md:gap-6">
        <div className="flex flex-col gap-2">
          <Label htmlFor="post-title" className="text-slate-900 text-sm font-medium">
            Post Title
          </Label>
          <Input
            id="post-title"
            placeholder="My Blog Post Title"
            value={blogData.title}
            onChange={(e) => handleInputChange("title", e.target.value)}
            className="p-3 rounded-lg border border-slate-200"
          />
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="excerpt" className="text-slate-900 text-sm font-medium">
            Excerpt
          </Label>
          <Textarea
            id="excerpt"
            placeholder="A brief summary of your post..."
            value={blogData.excerpt}
            onChange={(e) => handleInputChange("excerpt", e.target.value)}
            className="p-3 rounded-lg border border-slate-200 min-h-[60px] md:min-h-[80px]"
          />
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="content" className="text-slate-900 text-sm font-medium">
            Content
          </Label>
          <Textarea
            id="content"
            placeholder="Write your blog post content here..."
            value={blogData.content}
            onChange={(e) => handleInputChange("content", e.target.value)}
            className="p-3 rounded-lg border border-slate-200 min-h-[120px] md:min-h-[160px]"
          />
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="read-time" className="text-slate-900 text-sm font-medium">
            Read Time (minutes)
          </Label>
          <Input
            id="read-time"
            placeholder="5"
            type="number"
            value={blogData.readTime}
            onChange={(e) => handleInputChange("readTime", e.target.value)}
            className="p-3 rounded-lg border border-slate-200 w-full sm:w-32"
          />
        </div>

        <Button
          onClick={handleAddBlogPost}
          className="w-full px-3 py-3 bg-slate-900 hover:bg-slate-800 rounded-lg"
        >
          <span className="text-white text-sm">➕ Add Blog Post</span>
        </Button>
      </div>
    </div>
  )
} 