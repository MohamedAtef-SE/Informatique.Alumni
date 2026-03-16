import { useCallback, useRef, useState } from 'react';
import { GoogleMap, Marker, useLoadScript } from '@react-google-maps/api';
import { MapPin, Loader2, AlertCircle } from 'lucide-react';

const MAP_CONTAINER_STYLE = {
    width: '100%',
    height: '280px',
    borderRadius: '8px'
};

// Default center: Cairo, Egypt
const DEFAULT_CENTER = { lat: 30.0444, lng: 31.2357 };

const MAP_OPTIONS: google.maps.MapOptions = {
    disableDefaultUI: false,
    zoomControl: true,
    streetViewControl: false,
    mapTypeControl: false,
    fullscreenControl: false,
    clickableIcons: false,
    styles: [
        { elementType: 'geometry', stylers: [{ color: '#1e293b' }] },
        { elementType: 'labels.text.stroke', stylers: [{ color: '#1e293b' }] },
        { elementType: 'labels.text.fill', stylers: [{ color: '#94a3b8' }] },
        { featureType: 'road', elementType: 'geometry', stylers: [{ color: '#334155' }] },
        { featureType: 'road', elementType: 'geometry.stroke', stylers: [{ color: '#1e293b' }] },
        { featureType: 'water', elementType: 'geometry', stylers: [{ color: '#0f172a' }] },
        { featureType: 'poi.park', elementType: 'geometry', stylers: [{ color: '#1e3a2f' }] },
        { featureType: 'administrative', elementType: 'geometry.stroke', stylers: [{ color: '#334155' }] }
    ]
};

interface MapPickerProps {
    latitude?: number;
    longitude?: number;
    onLocationChange: (lat: number, lng: number) => void;
}

export function MapPicker({ latitude, longitude, onLocationChange }: MapPickerProps) {
    const apiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY as string;

    const { isLoaded, loadError } = useLoadScript({
        googleMapsApiKey: apiKey || '',
    });

    const hasCoords = typeof latitude === 'number' && typeof longitude === 'number';
    const [markerPos, setMarkerPos] = useState<google.maps.LatLngLiteral | null>(
        hasCoords ? { lat: latitude!, lng: longitude! } : null
    );

    const mapRef = useRef<google.maps.Map | null>(null);

    const center: google.maps.LatLngLiteral = hasCoords
        ? { lat: latitude!, lng: longitude! }
        : DEFAULT_CENTER;

    const onMapLoad = useCallback((map: google.maps.Map) => {
        mapRef.current = map;
    }, []);

    const handleMapClick = useCallback((e: google.maps.MapMouseEvent) => {
        if (!e.latLng) return;
        const lat = e.latLng.lat();
        const lng = e.latLng.lng();
        setMarkerPos({ lat, lng });
        onLocationChange(lat, lng);
    }, [onLocationChange]);

    // No API key configured
    if (!apiKey || apiKey === 'YOUR_GOOGLE_MAPS_API_KEY_HERE') {
        return (
            <div className="flex flex-col items-center justify-center gap-3 h-[280px] rounded-lg border border-dashed border-amber-500/40 bg-amber-500/5 text-amber-400">
                <AlertCircle className="w-8 h-8" />
                <div className="text-center">
                    <p className="text-sm font-medium">Google Maps API Key Required</p>
                    <p className="text-xs text-amber-500/70 mt-1">Add <code className="bg-amber-500/20 px-1 rounded">VITE_GOOGLE_MAPS_API_KEY</code> to your <code className="bg-amber-500/20 px-1 rounded">.env</code> file</p>
                </div>
            </div>
        );
    }

    if (loadError) {
        return (
            <div className="flex items-center justify-center gap-2 h-[280px] rounded-lg border border-red-500/30 bg-red-500/10 text-red-400">
                <AlertCircle className="w-5 h-5" />
                <span className="text-sm">Failed to load Google Maps. Check your API key.</span>
            </div>
        );
    }

    if (!isLoaded) {
        return (
            <div className="flex items-center justify-center gap-2 h-[280px] rounded-lg border border-slate-700 bg-slate-800/50">
                <Loader2 className="w-5 h-5 text-indigo-400 animate-spin" />
                <span className="text-sm text-slate-400">Loading map...</span>
            </div>
        );
    }

    return (
        <div className="space-y-2">
            <div className="relative rounded-lg overflow-hidden ring-1 ring-slate-700 hover:ring-indigo-500/50 transition-all">
                <GoogleMap
                    mapContainerStyle={MAP_CONTAINER_STYLE}
                    center={center}
                    zoom={hasCoords ? 14 : 10}
                    options={MAP_OPTIONS}
                    onLoad={onMapLoad}
                    onClick={handleMapClick}
                >
                    {markerPos && (
                        <Marker
                            position={markerPos}
                            animation={window.google?.maps?.Animation?.DROP}
                        />
                    )}
                </GoogleMap>

                {/* Instruction overlay */}
                {!markerPos && (
                    <div className="absolute bottom-3 left-1/2 -translate-x-1/2 bg-slate-900/90 backdrop-blur-sm text-slate-300 text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 border border-slate-700 pointer-events-none">
                        <MapPin className="w-3 h-3 text-indigo-400" />
                        Click on the map to set branch location
                    </div>
                )}
            </div>

            {/* Coordinate badge */}
            {markerPos && (
                <div className="flex items-center justify-between px-3 py-2 rounded-md bg-indigo-500/10 border border-indigo-500/20 text-xs">
                    <div className="flex items-center gap-2 text-indigo-300">
                        <MapPin className="w-3.5 h-3.5" />
                        <span className="font-medium">Location selected</span>
                    </div>
                    <span className="font-mono text-slate-400">
                        {markerPos.lat.toFixed(6)}, {markerPos.lng.toFixed(6)}
                    </span>
                </div>
            )}
        </div>
    );
}
