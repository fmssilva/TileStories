/**
 * NotFound Component - 404 Error Page
 * 
 * A friendly, helpful 404 page that follows UX best practices:
 * - Clear error message
 * - Helpful navigation options
 * - Maintains site branding
 * - Responsive design
 * - Theme-adaptive colors
 */

import { Link } from 'react-router-dom';
import { Button } from '@/components/ui';
import { useInlineTranslation } from '@/utils/language';

export function NotFound() {
    // Inline translations
    const title = useInlineTranslation('Página Não Encontrada', 'Page Not Found');
    const message = useInlineTranslation('Desculpe, não conseguimos encontrar a página que procura. Pode ter sido movida, eliminada ou inseriu um URL incorreto.', 'Sorry, we couldn\'t find the page you\'re looking for. It may have been moved, deleted, or you entered an incorrect URL.');
    const goHome = useInlineTranslation('Voltar ao Início', 'Go Home');
    const goBack = useInlineTranslation('Voltar', 'Go Back');
    const helpText = useInlineTranslation('Procura algo específico?', 'Looking for something specific?');
    const homeLink = useInlineTranslation('Início', 'Home');

    return (
        <div className="container mx-auto px-4 py-16 lg:py-24">
            <div className="max-w-2xl mx-auto text-center">
                {/* Error Code */}
                <div className="mb-8">
                    <h1 className="text-8xl lg:text-9xl font-bold text-primary/20 mb-4 select-none">
                        404
                    </h1>
                    <h2 className="text-3xl lg:text-4xl font-bold text-foreground mb-4">
                        {title}
                    </h2>
                    <p className="text-lg text-muted-foreground mb-8 max-w-md mx-auto">
                        {message}
                    </p>
                </div>

                {/* Action Buttons */}
                <div className="flex flex-col sm:flex-row gap-4 justify-center items-center mb-12">
                    <Button asChild variant="primary" size="lg">
                        <Link to="/">
                            {goHome}
                        </Link>
                    </Button>

                    <Button
                        variant="outline"
                        size="lg"
                        onClick={() => window.history.back()}
                    >
                        {goBack}
                    </Button>
                </div>                {/* Helpful Links */}
                <div className="border-t border-border pt-8">
                    <p className="text-sm text-muted-foreground mb-4">
                        {helpText}
                    </p>
                    <div className="flex flex-wrap justify-center gap-4 text-sm">
                        <Link
                            to="/"
                            className="text-primary hover:text-primary/80 hover:underline"
                        >
                            {homeLink}
                        </Link>
                        {/* Add more helpful links as your site grows */}
                        {/* 
                        <Link 
                            to="/about" 
                            className="text-primary hover:text-primary/80 hover:underline"
                        >
                            About
                        </Link>
                        <Link 
                            to="/contact" 
                            className="text-primary hover:text-primary/80 hover:underline"
                        >
                            Contact
                        </Link>
                        */}
                    </div>
                </div>
            </div>
        </div>
    );
}