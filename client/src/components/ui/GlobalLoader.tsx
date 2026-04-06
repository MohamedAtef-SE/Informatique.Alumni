import { useLoaderStore } from '../../stores/useLoaderStore';
import LoadingLayer from '../common/LoadingLayer';

const GlobalLoader = () => {
    const { isLoading } = useLoaderStore();

    if (!isLoading) return null;

    return <LoadingLayer />;
};

export default GlobalLoader;
