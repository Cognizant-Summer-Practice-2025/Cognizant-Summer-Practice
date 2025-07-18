"use client"

import { useState, useEffect } from 'react';
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { SkillDropdown } from "@/components/ui/skill-dropdown"
import { Skill } from '@/lib/portfolio';
import { getSkillsByPortfolioId, createSkill, updateSkill, deleteSkill } from '@/lib/portfolio/api';

interface SkillsProps {
  portfolioId?: string;
  initialSkills?: Skill[];
  readOnly?: boolean;
  onSkillsUpdate?: () => Promise<void>;
}

export default function Skills({ portfolioId, initialSkills, readOnly = false, onSkillsUpdate }: SkillsProps = {}) {
  const [skills, setSkills] = useState<Skill[]>(initialSkills || []);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [newProficiency, setNewProficiency] = useState(50);
  const [selectedSkillData, setSelectedSkillData] = useState<{
    categoryType: string
    subcategory: string
    skillName: string
    fullCategoryPath: string
  } | null>(null);
  const [editingSkill, setEditingSkill] = useState<string | null>(null);
  const [editForm, setEditForm] = useState<Partial<Skill>>({});

  // Fetch skills from API when portfolioId is provided
  useEffect(() => {
    async function fetchSkills() {
      if (!portfolioId || initialSkills) return;
      
      try {
        setLoading(true);
        setError(null);
        const fetchedSkills = await getSkillsByPortfolioId(portfolioId);
        setSkills(fetchedSkills);
      } catch (err) {
        console.error('Error fetching skills:', err);
        setError('Failed to load skills');
      } finally {
        setLoading(false);
      }
    }

    fetchSkills();
  }, [portfolioId, initialSkills]);

  const handleSkillSelect = (skillData: {
    categoryType: string
    subcategory: string
    skillName: string
    fullCategoryPath: string
  }) => {
    setSelectedSkillData(skillData);
    setError(null); // Clear any previous errors when a skill is selected
  };

  const addSkill = async () => {
    if (!selectedSkillData || !portfolioId) return;
    
    if (skills.some(skill => skill.name.toLowerCase() === selectedSkillData.skillName.toLowerCase())) {
      setError('Skill already exists');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
      const newSkillData = {
        portfolioId,
        name: selectedSkillData.skillName,
        categoryType: selectedSkillData.categoryType,
        subcategory: selectedSkillData.subcategory,
        category: selectedSkillData.fullCategoryPath,
        proficiencyLevel: newProficiency,
        displayOrder: skills.length + 1,
      };

      const createdSkill = await createSkill(newSkillData);
      setSkills([...skills, createdSkill]);
      setSelectedSkillData(null);
      setNewProficiency(50);
      onSkillsUpdate?.();
    } catch (err) {
      console.error('Error creating skill:', err);
      setError('Failed to add skill');
    } finally {
      setLoading(false);
    }
  };

  const updateExistingSkill = async (skillId: string, updates: Partial<Skill>) => {
    if (!portfolioId) return;

    try {
      setLoading(true);
      setError(null);
      
      const updatedSkill = await updateSkill(skillId, updates);
      setSkills(skills.map(skill => skill.id === skillId ? updatedSkill : skill));
      setEditingSkill(null);
      setEditForm({});
      onSkillsUpdate?.();
    } catch (err) {
      console.error('Error updating skill:', err);
      setError('Failed to update skill');
    } finally {
      setLoading(false);
    }
  };

  const removeSkill = async (skillId: string) => {
    try {
      setLoading(true);
      setError(null);
      
      await deleteSkill(skillId);
      setSkills(skills.filter(skill => skill.id !== skillId));
      onSkillsUpdate?.();
    } catch (err) {
      console.error('Error deleting skill:', err);
      setError('Failed to delete skill');
    } finally {
      setLoading(false);
    }
  };

  const startEditing = (skill: Skill) => {
    setEditingSkill(skill.id);
    setEditForm({
      name: skill.name,
      category: skill.category,
      proficiencyLevel: skill.proficiencyLevel,
    });
  };

  const cancelEditing = () => {
    setEditingSkill(null);
    setEditForm({});
  };

  const saveEdit = () => {
    if (editingSkill && editForm.name?.trim()) {
      updateExistingSkill(editingSkill, editForm);
    }
  };

  const getProficiencyColor = (level?: number) => {
    if (!level) return 'bg-gray-200';
    if (level >= 80) return 'bg-green-500';
    if (level >= 60) return 'bg-blue-500';
    if (level >= 40) return 'bg-yellow-500';
    return 'bg-red-500';
  };

  const getProficiencyLabel = (level?: number) => {
    if (!level) return 'Unknown';
    if (level >= 80) return 'Expert';
    if (level >= 60) return 'Advanced';
    if (level >= 40) return 'Intermediate';
    return 'Beginner';
  };

  if (loading && skills.length === 0) {
    return (
      <div className="space-y-6">
        <h2 className="text-2xl font-bold mb-4">Skills</h2>
        <div className="flex items-center justify-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
          <span className="ml-2">Loading skills...</span>
        </div>
      </div>
    );
    }

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-bold mb-4">Skills</h2>
      
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
          {error}
          <button 
            onClick={() => setError(null)}
            className="ml-2 text-red-500 hover:text-red-700"
          >
            ×
          </button>
        </div>
      )}

      {!readOnly && portfolioId && (
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-lg font-medium mb-3">Add New Skill</h3>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Skill Selection */}
            <div>
              <SkillDropdown
                onSkillSelect={handleSkillSelect}
                disabled={loading}
                className="w-full"
              />
            </div>
            
            {/* Proficiency Selection */}
            <div className="space-y-4">
              <div>
                <Label htmlFor="skill-proficiency">Proficiency ({newProficiency}%)</Label>
                <input
                  id="skill-proficiency"
                  type="range"
                  min="1"
                  max="100"
                  value={newProficiency}
                  onChange={(e) => setNewProficiency(parseInt(e.target.value))}
                  className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer mt-1"
                  disabled={loading}
                />
              </div>
              
              <div className="flex items-end">
                <Button
                  onClick={addSkill}
                  disabled={!selectedSkillData || loading}
                  className="w-full"
                >
                  {loading ? 'Adding...' : 'Add Skill'}
                </Button>
              </div>
            </div>
          </div>

          {/* Selected Skill Preview */}
          {selectedSkillData && (
            <div className="mt-4 p-3 bg-blue-50 border border-blue-200 rounded-lg">
              <p className="text-sm text-blue-800">
                <strong>Selected:</strong> {selectedSkillData.skillName} 
                <span className="text-blue-600"> ({selectedSkillData.fullCategoryPath})</span>
              </p>
            </div>
          )}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {skills.length === 0 ? (
          <div className="col-span-full text-center py-8 text-gray-500">
            {portfolioId ? 'No skills added yet.' : 'No skills data available.'}
                  </div>
        ) : (
          skills
            .sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0))
            .map((skill) => (
              <div key={skill.id} className="bg-white border rounded-lg p-4 hover:shadow-md transition-shadow">
                {editingSkill === skill.id ? (
                  <div className="space-y-3">
                    <p className="text-sm font-medium text-gray-700">
                      Skill: {editForm.name}
                    </p>
                    <p className="text-sm text-gray-600">
                      Category: {editForm.category}
                    </p>
                    <div>
                      <Label>Proficiency ({editForm.proficiencyLevel || skill.proficiencyLevel}%)</Label>
                  <input
                    type="range"
                    min="1"
                    max="100"
                        value={editForm.proficiencyLevel || skill.proficiencyLevel}
                        onChange={(e) => setEditForm({...editForm, proficiencyLevel: parseInt(e.target.value)})}
                    className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
                  />
                </div>
                    <div className="flex gap-2">
                      <Button size="sm" onClick={saveEdit} disabled={loading}>
                        Save
                      </Button>
                      <Button size="sm" variant="outline" onClick={cancelEditing}>
                        Cancel
                      </Button>
              </div>
            </div>
                ) : (
                  <>
                    <div className="flex justify-between items-start mb-2">
                      <h3 className="font-medium">{skill.name}</h3>
                      {!readOnly && (
                        <div className="flex gap-1">
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => startEditing(skill)}
                            disabled={loading}
                          >
                            Edit
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => removeSkill(skill.id)}
                            disabled={loading}
                            className="text-red-600 hover:text-red-800"
                          >
                            Delete
                          </Button>
            </div>
          )}
        </div>
                    <p className="text-sm text-gray-600 mb-2">{skill.category}</p>
                    <div className="space-y-1">
                      <div className="flex justify-between text-sm">
                        <span>{getProficiencyLabel(skill.proficiencyLevel)}</span>
                        <span>{skill.proficiencyLevel || 0}%</span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className={`h-2 rounded-full ${getProficiencyColor(skill.proficiencyLevel)}`}
                          style={{ width: `${skill.proficiencyLevel || 0}%` }}
                        ></div>
                      </div>
                    </div>
                  </>
                )}
              </div>
            ))
        )}
      </div>
    </div>
  );
} 