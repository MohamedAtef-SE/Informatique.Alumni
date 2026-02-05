import { useState, useCallback } from 'react';
import Cropper from 'react-easy-crop';
import { Button } from '../ui/Button';
import getCroppedImg from '../../utils/canvasUtils';
import { Check, X } from 'lucide-react';

interface ImageCropperProps {
    imageSrc: string;
    onCropComplete: (croppedBlob: Blob) => void;
    onCancel: () => void;
}

const ImageCropper = ({ imageSrc, onCropComplete, onCancel }: ImageCropperProps) => {
    const [crop, setCrop] = useState({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [croppedAreaPixels, setCroppedAreaPixels] = useState<any>(null);

    const onCropChange = (crop: { x: number; y: number }) => {
        setCrop(crop);
    };

    const onZoomChange = (zoom: number) => {
        setZoom(zoom);
    };

    const onCropCompleteHandler = useCallback((_: any, croppedAreaPixels: any) => {
        setCroppedAreaPixels(croppedAreaPixels);
    }, []);

    const handleSave = async () => {
        if (imageSrc && croppedAreaPixels) {
            try {
                const croppedImage = await getCroppedImg(imageSrc, croppedAreaPixels);
                if (croppedImage) {
                    onCropComplete(croppedImage);
                }
            } catch (e) {
                console.error(e);
            }
        }
    };

    return (
        <div className="flex flex-col h-[500px] w-full relative">
            <div className="flex-1 relative bg-slate-900 overflow-hidden rounded-t-lg">
                <Cropper
                    image={imageSrc}
                    crop={crop}
                    zoom={zoom}
                    aspect={1}
                    onCropChange={onCropChange}
                    onCropComplete={onCropCompleteHandler}
                    onZoomChange={onZoomChange}
                />
            </div>

            <div className="p-4 bg-slate-950 border-t border-white/10 space-y-4">
                <div className="flex items-center gap-4">
                    <span className="text-xs text-slate-400 font-medium w-12">Zoom</span>
                    <input
                        type="range"
                        value={zoom}
                        min={1}
                        max={3}
                        step={0.1}
                        aria-labelledby="Zoom"
                        onChange={(e) => setZoom(Number(e.target.value))}
                        className="w-full h-1 bg-slate-700 rounded-lg appearance-none cursor-pointer accent-accent"
                    />
                </div>

                <div className="flex justify-end gap-3 pt-2">
                    <Button variant="ghost" onClick={onCancel}>
                        <X className="w-4 h-4 mr-2" /> Cancel
                    </Button>
                    <Button onClick={handleSave}>
                        <Check className="w-4 h-4 mr-2" /> Save Photo
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default ImageCropper;
