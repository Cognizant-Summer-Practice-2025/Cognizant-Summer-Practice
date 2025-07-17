"use client"

import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { useDraft } from "@/lib/contexts/draft-context"

export function AddBlogPost() {
  const { addDraftBlogPost } = useDraft()
  
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [blogPostData, setBlogPostData] = useState({
    title: "",
    content: "",
    featuredImageUrl: "",
    tags: "",
    publishImmediately: true
  })

  const handleInputChange = (field: string, value: string | boolean) => {
    setBlogPostData(prev => ({ ...prev, [field]: value }))
    if (error) setError(null)
    if (success) setSuccess(false)
  }

  const handleAddBlogPost = async () => {
    if (!blogPostData.title.trim()) {
      setError("Please enter a blog post title.")
      return
    }

    if (!blogPostData.content.trim()) {
      setError("Please enter blog post content.")
      return
    }

    try {
      setLoading(true)
      setError(null)

      // Save to draft context instead of API
      addDraftBlogPost({
        title: blogPostData.title.trim(),
        content: blogPostData.content.trim(),
        featuredImageUrl: blogPostData.featuredImageUrl.trim(),
        tags: blogPostData.tags.trim(),
        publishImmediately: blogPostData.publishImmediately
      })

      // Reset form
      setBlogPostData({
        title: "",
        content: "",
        featuredImageUrl: "",
        tags: "",
        publishImmediately: true
      })

      setSuccess(true)
      
      // Hide success message after 3 seconds
      setTimeout(() => setSuccess(false), 3000)

    } catch (error) {
      console.error('Error adding blog post draft:', error)
      setError('Failed to add blog post. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="border border-slate-200 rounded-lg p-6 bg-white">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-xl font-semibold text-slate-900">Add New Blog Post</h3>
          <p className="text-sm text-slate-600 mt-1">
            Blog post will be saved as draft and published when you hit publish
          </p>
        </div>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-sm text-red-600">{error}</p>
        </div>
      )}

      {success && (
        <div className="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg">
          <p className="text-sm text-green-600">✅ Blog post added to drafts!</p>
        </div>
      )}

      <div className="grid grid-cols-1 gap-6">
        <div>
          <Label htmlFor="title" className="text-sm font-medium text-slate-700">
            Title *
          </Label>
          <Input
            id="title"
            value={blogPostData.title}
            onChange={(e) => handleInputChange("title", e.target.value)}
            placeholder="Enter blog post title"
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="content" className="text-sm font-medium text-slate-700">
            Content *
          </Label>
          <Textarea
            id="content"
            value={blogPostData.content}
            onChange={(e) => handleInputChange("content", e.target.value)}
            placeholder="Write your blog post content..."
            className="mt-1"
            rows={8}
          />
        </div>

        <div>
          <Label htmlFor="featuredImageUrl" className="text-sm font-medium text-slate-700">
            Featured Image URL
          </Label>
          <Input
            id="featuredImageUrl"
            value={blogPostData.featuredImageUrl}
            onChange={(e) => handleInputChange("featuredImageUrl", e.target.value)}
            placeholder="https://example.com/image.jpg"
            className="mt-1"
          />
        </div>

        <div>
          <Label htmlFor="tags" className="text-sm font-medium text-slate-700">
            Tags
          </Label>
          <Input
            id="tags"
            value={blogPostData.tags}
            onChange={(e) => handleInputChange("tags", e.target.value)}
            placeholder="React, JavaScript, Web Development (comma-separated)"
            className="mt-1"
          />
        </div>

        <div className="flex items-center space-x-2">
          <Checkbox
            id="publishImmediately"
            checked={blogPostData.publishImmediately}
            onCheckedChange={(checked) => handleInputChange("publishImmediately", checked === true)}
          />
          <Label htmlFor="publishImmediately" className="text-sm font-medium text-slate-700">
            Publish immediately when portfolio is published
          </Label>
        </div>

        <div className="flex justify-end pt-4">
          <Button
            onClick={handleAddBlogPost}
            disabled={loading}
            className="px-6 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg"
          >
            {loading ? "Adding..." : "Add Blog Post"}
          </Button>
        </div>
      </div>
    </div>
  )
} 