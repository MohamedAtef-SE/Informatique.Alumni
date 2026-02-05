import { useQuery } from '@tanstack/react-query';
import { adminService } from '../../services/adminService';
import { Plus, Plane, MapPin } from 'lucide-react';

const TripsManager = () => {
    const { data } = useQuery({
        queryKey: ['admin-trips'],
        queryFn: () => adminService.getTrips({ maxResultCount: 20 })
    });

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold text-white">Trips Manager</h1>
                <button className="btn-primary flex items-center gap-2 px-4 py-2">
                    <Plus className="w-4 h-4" /> New Trip
                </button>
            </div>

            <div className="glass-panel overflow-hidden">
                <table className="w-full text-left text-sm">
                    <thead className="bg-white/5 text-gray-400 uppercase tracking-wider text-xs">
                        <tr>
                            <th className="px-6 py-4">Title</th>
                            <th className="px-6 py-4">Destination</th>
                            <th className="px-6 py-4">Dates</th>
                            <th className="px-6 py-4">Capacity</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-white/5">


                        {data?.items.map((trip: any) => (
                            <tr key={trip.id} className="hover:bg-white/5 transition-colors">
                                <td className="px-6 py-4 text-white font-medium flex items-center gap-2">
                                    <Plane className="w-4 h-4 text-sky-400" />
                                    {trip.title}
                                </td>
                                <td className="px-6 py-4 text-gray-300 flex items-center gap-1">
                                    <MapPin className="w-3 h-3 text-gray-500" />
                                    {trip.destination}
                                </td>
                                <td className="px-6 py-4 text-gray-400 font-mono text-xs">
                                    {new Date(trip.startDate).toLocaleDateString()}
                                </td>
                                <td className="px-6 py-4 text-gray-300">
                                    Max: {trip.maxCapacity}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default TripsManager;
