import React from 'react';
import type { Experience } from '@/lib/portfolio';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Calendar, Building } from 'lucide-react';

interface ExperienceProps {
  data: Experience[];
}

export function Experience({ data: experiences }: ExperienceProps) {
  if (!experiences || experiences.length === 0) {
    return (
      <div className="text-center text-muted-foreground">
        No experience data available.
      </div>
    );
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      year: 'numeric'
    });
  };

  const totalCount = experiences.length;

  return (
    <div className="modern-component-container">
      {/* Count indicator */}
      <div className="mb-4 pb-2 border-b border-border">
        <p className="text-sm text-muted-foreground">
          {totalCount} experience{totalCount !== 1 ? 's' : ''}
        </p>
      </div>

      <div className="max-h-[800px] overflow-y-auto pr-2">
        <div className="modern-grid single-column">
          {experiences.map((exp) => (
            <Card key={exp.id} className="modern-card">
              <CardContent className="p-6">
                <div className="flex flex-col md:flex-row md:items-start md:justify-between mb-4">
                  <div className="flex-1">
                    <h3 className="text-xl font-semibold text-foreground mb-1">
                      {exp.jobTitle}
                    </h3>
                    <div className="flex items-center gap-2 mb-2">
                      <Building size={16} className="text-muted-foreground" />
                      <span className="text-lg font-medium text-muted-foreground">
                        {exp.companyName}
                      </span>
                    </div>
                  </div>
                  
                  <div className="flex items-center gap-2 text-sm text-muted-foreground">
                    <Calendar size={14} />
                    <span>
                      {formatDate(exp.startDate)} - {exp.isCurrent ? 'Present' : formatDate(exp.endDate!)}
                    </span>
                  </div>
                </div>

                {exp.description && (
                  <p className="text-muted-foreground mb-4 leading-relaxed">
                    {exp.description}
                  </p>
                )}

                {exp.skillsUsed && exp.skillsUsed.length > 0 && (
                  <div className="flex flex-wrap gap-2">
                    {exp.skillsUsed.map((skill, index) => (
                      <Badge key={index} variant="outline" className="modern-skill-tag">
                        {skill}
                      </Badge>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
} 