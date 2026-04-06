import React from 'react';
import { Card, CardContent } from '../ui/Card';

/**
 * AlumniCardSkeleton
 * A high-end skeleton loader for the Alumni Directory.
 * Uses pulsing animations and brand-themed shapes to improve perceived performance.
 */
const AlumniCardSkeleton: React.FC = () => {
  return (
    <Card className="overflow-hidden border-[var(--color-border)] h-full animate-pulse">
      <CardContent className="p-6 flex flex-col items-center">
        {/* Profile Circle Skeleton */}
        <div className="relative mb-5">
          <div className="w-24 h-24 rounded-full bg-slate-200" />
          <div className="absolute -bottom-1 left-1.2 -translate-x-1/2 w-10 h-4 bg-slate-200 rounded-full border border-white" />
        </div>

        {/* Title & Subtitle Skeletons */}
        <div className="h-6 w-3/4 bg-slate-200 rounded-md mb-2" />
        <div className="h-4 w-1/2 bg-slate-100 rounded-md mb-4" />

        {/* Divider */}
        <div className="w-full h-px bg-[var(--color-border)] my-4" />

        {/* Meta Info Skeletons */}
        <div className="w-full space-y-4 flex-1">
          <div className="flex justify-between items-center">
            <div className="h-3 w-16 bg-slate-100 rounded-sm" />
            <div className="h-3 w-8 bg-slate-200 rounded-sm" />
          </div>
          <div className="flex justify-between items-center">
            <div className="h-3 w-20 bg-slate-100 rounded-sm" />
            <div className="h-3 w-24 bg-slate-200 rounded-sm" />
          </div>
        </div>

        {/* Button Skeleton */}
        <div className="w-full h-9 mt-6 bg-slate-100 rounded-md" />
      </CardContent>
    </Card>
  );
};

export default AlumniCardSkeleton;
