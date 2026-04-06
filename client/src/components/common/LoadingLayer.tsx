import React from 'react';

/**
 * Premium Loading Layer
 * A state-of-the-art loading experience with glassmorphism and unique CSS animations.
 * Replaces generic "Loading..." text with a high-end visual language.
 */
const LoadingLayer: React.FC = () => {
  return (
    <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-white/40 backdrop-blur-md animate-fade-in">
      <div className="relative flex items-center justify-center">
        {/* Outer Pulsating Ring */}
        <div className="absolute h-24 w-24 rounded-full border-4 border-accent animate-ping opacity-20" />
        
        {/* Middle Sequential Ring */}
        <div className="absolute h-16 w-16 rounded-full border-b-4 border-l-4 border-accent animate-spin duration-[1500ms]" />
        
        {/* Functional Core Shape */}
        <div className="relative flex h-10 w-10 items-center justify-center">
          <div className="h-4 w-4 rotate-45 rounded-sm bg-accent animate-pulse shadow-accent" />
          <div className="absolute h-8 w-8 rounded-full border-2 border-accent/30 animate-pulse delay-75" />
        </div>
      </div>
      
      {/* Decorative Shimmer Overlay (optional but adds premium feel) */}
      <div className="pointer-events-none absolute inset-0 overflow-hidden opacity-10">
        <div className="h-full w-full animate-gradient bg-gradient-to-r from-transparent via-white/50 to-transparent bg-[length:200%_100%]" />
      </div>

      <style>{`
        @keyframes fade-in {
          from { opacity: 0; }
          to { opacity: 1; }
        }
        .animate-fade-in {
          animation: fade-in 0.3s ease-out forwards;
        }
        .shadow-accent {
          box-shadow: 0 0 15px rgba(45, 150, 215, 0.6);
        }
      `}</style>
    </div>
  );
};

export default LoadingLayer;
