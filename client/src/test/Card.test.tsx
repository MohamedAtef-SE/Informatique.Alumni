import { render, screen } from '@testing-library/react';
import { Card, CardContent } from '../components/ui/Card'; // Corrected path
import { describe, it, expect } from 'vitest';


describe('Card Component', () => {
    it('renders children correctly', () => {
        render(
            <Card>
                <CardContent>Test Content</CardContent>
            </Card>
        );
        expect(screen.getByText('Test Content')).toBeInTheDocument();
    });

    it('applies variant classes', () => {
        const { container } = render(
            <Card variant="elevated">
                <CardContent>Elevated</CardContent>
            </Card>
        );
        // Adjust expectation based on actual class implementation in Card.tsx
        // variant="elevated" adds "shadow-lg"
        expect(container.firstChild).toHaveClass('shadow-lg');
    });
});
