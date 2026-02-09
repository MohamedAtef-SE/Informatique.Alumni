import { motion, useScroll, useTransform, useSpring, useMotionValue, animate, useInView } from 'framer-motion';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Calendar, Users, Briefcase, GraduationCap, Building2, Newspaper, ChevronRight } from 'lucide-react';
import { servicesAppService } from '../../services/servicesService';
import { eventsService } from '../../services/eventsService';
import { useRef, useEffect } from 'react';
import { Button } from '../../components/ui/Button';

import { Card, CardContent } from '../../components/ui/Card';

const PortalHome = () => {
    const { t } = useTranslation();

    // Dynamic Data Fetching
    const { data: news } = useQuery({
        queryKey: ['latest-news'],
        queryFn: () => servicesAppService.getPosts()
    });

    const { data: events } = useQuery({
        queryKey: ['upcoming-events'],
        queryFn: () => eventsService.getList({ maxResultCount: 3 })
    });

    // Parallax & Scroll Effects
    const heroRef = useRef<HTMLDivElement>(null);
    const { scrollYProgress } = useScroll({
        target: heroRef,
        offset: ["start start", "end start"]
    });

    const yBackground = useTransform(scrollYProgress, [0, 1], ["0%", "50%"]);
    const opacityHero = useTransform(scrollYProgress, [0, 0.8], [1, 0]);
    const scaleHero = useTransform(scrollYProgress, [0, 1], [1, 1.1]);

    // Spring physics for smooth scroll connection
    const textY = useSpring(useTransform(scrollYProgress, [0, 1], [0, 100]), {
        stiffness: 100,
        damping: 30
    });

    // Stagger Configuration
    const containerVariants = {
        hidden: { opacity: 0 },
        visible: {
            opacity: 1,
            transition: {
                staggerChildren: 0.15,
                delayChildren: 0.2
            }
        }
    };

    return (
        <div className="flex flex-col min-h-screen bg-[var(--color-background)]">
            {/* Parallax Hero Section */}
            <section ref={heroRef} className="relative h-[90vh] flex items-center justify-center overflow-hidden">
                <motion.div
                    style={{ y: yBackground, scale: scaleHero }}
                    className="absolute inset-0 z-0"
                >
                    <img
                        src="https://images.unsplash.com/photo-1541339907198-e08756dedf3f?q=80&w=2070&auto=format&fit=crop"
                        alt="University Campus"
                        className="w-full h-full object-cover opacity-60"
                    />
                    <div className="absolute inset-0 bg-gradient-to-b from-white/10 via-white/40 to-[var(--color-background)]" />
                </motion.div>

                <div className="relative z-10 container mx-auto px-4 lg:px-8 pt-20 text-center">
                    <motion.div
                        style={{ y: textY, opacity: opacityHero }}
                        initial={{ opacity: 0, y: 50, filter: "blur(10px)" }}
                        animate={{ opacity: 1, y: 0, filter: "blur(0px)" }}
                        transition={{ duration: 1, ease: "easeOut" }}
                    >
                        <motion.h1
                            className="text-5xl md:text-8xl font-black text-[var(--color-text-primary)] mb-8 tracking-tighter"
                            initial={{ scale: 0.9 }}
                            animate={{ scale: 1 }}
                            transition={{ duration: 1.2, ease: "anticipate" }}
                        >
                            {t('home.hero.welcome')} <br />
                            <span className="text-gradient">
                                {t('home.hero.alumni')}
                            </span>
                        </motion.h1>

                        <p className="text-xl md:text-2xl text-[var(--color-text-secondary)] max-w-2xl mx-auto mb-12 leading-relaxed font-light">
                            {t('home.hero.subtitle')}
                        </p>

                        <div className="flex flex-col sm:flex-row gap-6 justify-center">
                            <Link to="/portal/directory">
                                <motion.button
                                    whileHover={{ scale: 1.05 }}
                                    whileTap={{ scale: 0.95 }}
                                    className="btn-primary px-10 py-4 text-lg"
                                >
                                    {t('home.hero.find_classmates')}
                                </motion.button>
                            </Link>
                            <Link to="/portal/events">
                                <motion.button
                                    whileHover={{ scale: 1.05, backgroundColor: "var(--color-secondary)" }}
                                    whileTap={{ scale: 0.95 }}
                                    className="px-10 py-4 rounded-xl bg-white text-[var(--color-text-primary)] shadow-sm border border-[var(--color-border)] font-bold text-lg transition-colors"
                                >
                                    {t('home.hero.browse_events')}
                                </motion.button>
                            </Link>
                        </div>
                    </motion.div>
                </div>

                {/* Scroll Down Indicator */}
                <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1, y: [0, 10, 0] }}
                    transition={{ delay: 2, duration: 2, repeat: Infinity }}
                    className="absolute bottom-10 left-1/2 -translate-x-1/2 text-[var(--color-text-muted)]"
                >
                    <div className="w-1 h-12 rounded-full bg-gradient-to-b from-slate-200/0 via-slate-300 to-slate-200/0" />
                </motion.div>
            </section>

            {/* Quick Access Grid */}
            <section className="py-24 -mt-32 relative z-20 container mx-auto px-4 lg:px-8 pointer-events-none">
                <motion.div
                    variants={containerVariants}
                    initial="hidden"
                    whileInView="visible"
                    viewport={{ once: false, amount: 0.25 }}
                    className="grid grid-cols-1 md:grid-cols-3 gap-8 pointer-events-auto"
                >
                    <FeatureCard
                        to="/portal/services"
                        icon={<GraduationCap className="w-10 h-10 text-[var(--color-accent)]" />}
                        title={t('home.features.services.title')}
                        desc={t('home.features.services.desc')}
                        color="accent"
                        exploreText={t('home.features.explore')}
                    />
                    <FeatureCard
                        to="/portal/career"
                        icon={<Briefcase className="w-10 h-10 text-emerald-500" />}
                        title={t('home.features.career.title')}
                        desc={t('home.features.career.desc')}
                        color="emerald"
                        exploreText={t('home.features.explore')}
                    />
                    <FeatureCard
                        to="/portal/gallery"
                        icon={<Users className="w-10 h-10 text-blue-500" />}
                        title={t('home.features.gallery.title')}
                        desc={t('home.features.gallery.desc')}
                        color="blue"
                        exploreText={t('home.features.explore')}
                    />
                </motion.div>
            </section>



            {/* About Our Alumni Section */}
            <section className="py-24 relative overflow-hidden bg-white">
                {/* Decorative Background Elements */}
                <div className="absolute top-0 left-0 w-full h-full pointer-events-none">
                    <div className="absolute top-20 -left-40 w-80 h-80 bg-[var(--color-accent-light)] rounded-full blur-3xl animate-pulse" />
                    <div className="absolute bottom-20 -right-40 w-96 h-96 bg-blue-50/20 rounded-full blur-3xl opacity-50" />
                </div>

                <div className="container mx-auto px-4 lg:px-8 relative z-10">
                    <motion.div
                        initial={{ opacity: 0, y: 30 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: false, amount: 0.25 }}
                        transition={{ duration: 0.8 }}
                        className="max-w-4xl"
                    >
                        <motion.div
                            initial={{ width: 0 }}
                            whileInView={{ width: "100px" }}
                            viewport={{ once: false }}
                            transition={{ duration: 0.6, delay: 0.2 }}
                            className="h-1 bg-gradient-to-r from-[var(--color-accent)] to-emerald-400 mb-6"
                        />
                        <h2 className="text-4xl md:text-6xl font-bold text-[var(--color-text-primary)] mb-8 leading-tight">
                            {t('home.about.title_prefix')}{' '}
                            <motion.span
                                initial={{ opacity: 0, x: -20 }}
                                whileInView={{ opacity: 1, x: 0 }}
                                viewport={{ once: false }}
                                transition={{ delay: 0.3 }}
                                className="text-gradient"
                            >
                                {t('home.about.title_suffix')}
                            </motion.span>
                        </h2>
                        <motion.p
                            initial={{ opacity: 0, y: 20 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: false }}
                            transition={{ delay: 0.4 }}
                            className="text-lg md:text-xl text-[var(--color-text-secondary)] leading-relaxed bg-slate-50 p-8 rounded-2xl border border-[var(--color-border)] shadow-sm"
                        >
                            {t('home.about.description')}
                        </motion.p>
                    </motion.div>
                </div>
            </section>

            {/* Path to Success - Statistics */}
            <section className="py-24 bg-slate-50/50 border-y border-[var(--color-border)] relative">
                <div className="container mx-auto px-4 lg:px-8 relative z-10">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: false, amount: 0.25 }}
                        className="text-center mb-20"
                    >
                        <h2 className="text-3xl md:text-5xl font-bold text-[var(--color-text-primary)] mb-4">
                            {t('home.stats.title_prefix')}{' '}
                            <span className="text-gradient">
                                {t('home.stats.title_suffix')}
                            </span>
                        </h2>
                        <p className="text-[var(--color-text-secondary)] max-w-2xl mx-auto">
                            {t('home.stats.subtitle')}
                        </p>
                    </motion.div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-8 md:gap-12">
                        <StatCard value="96%" label={t('home.stats.employed')} delay={0} icon="💼" />
                        <StatCard value="10K+" label={t('home.stats.global')} delay={0.1} icon="🌍" />
                        <StatCard value="85%" label={t('home.stats.donations')} delay={0.2} icon="🎁" />
                        <StatCard value="50+" label={t('home.stats.countries')} delay={0.3} icon="🏠" />
                    </div>
                </div>
            </section>

            {/* Notable Alumni Section */}
            <section className="py-24 container mx-auto px-4 lg:px-8">
                <motion.div
                    initial={{ opacity: 0, x: -20 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: false, amount: 0.25 }}
                    className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-12"
                >
                    <h2 className="text-4xl font-bold text-[var(--color-text-primary)]">
                        {t('home.notable.title_prefix')} <span className="text-gradient">{t('home.notable.title_suffix')}</span>
                    </h2>
                    <Link to="/portal/directory">
                        <Button variant="outline" className="group rounded-full px-8">
                            {t('home.notable.more_btn')} <ChevronRight className="w-4 h-4 group-hover:translate-x-1 transition-transform ml-2 rtl:rotate-180 rtl:group-hover:-translate-x-1 rtl:ml-0 rtl:mr-2" />
                        </Button>
                    </Link>
                </motion.div>

                <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                    <AlumniCard
                        name="Dr. Ahmed Hassan"
                        year="95"
                        title="CEO, Tech Innovations"
                        image="https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=400&fit=crop"
                        delay={0}
                        viewProfileText={t('home.notable.view_profile')}
                        classOfText={t('home.notable.class_of')}
                    />
                    <AlumniCard
                        name="Eng. Sarah Mohamed"
                        year="99"
                        title="Minister of Technology"
                        image="https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=400&h=400&fit=crop"
                        delay={0.1}
                        viewProfileText={t('home.notable.view_profile')}
                        classOfText={t('home.notable.class_of')}
                    />
                    <AlumniCard
                        name="Dr. Khaled Ibrahim"
                        year="02"
                        title="World Bank Director"
                        image="https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&h=400&fit=crop"
                        delay={0.2}
                        viewProfileText={t('home.notable.view_profile')}
                        classOfText={t('home.notable.class_of')}
                    />
                    <AlumniCard
                        name="Prof. Nadia El-Sayed"
                        year="98"
                        title="University President"
                        image="https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=400&h=400&fit=crop"
                        delay={0.3}
                        viewProfileText={t('home.notable.view_profile')}
                        classOfText={t('home.notable.class_of')}
                    />
                </div>
            </section>

            {/* Alumni Success Stories */}
            <section className="py-24 bg-slate-50 border-y border-[var(--color-border)] relative overflow-hidden">
                <div className="absolute -top-40 right-0 w-96 h-96 bg-[var(--color-accent-light)] rounded-full blur-3xl pointer-events-none opacity-40" />

                <div className="container mx-auto px-4 lg:px-8 relative z-10">
                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: false, amount: 0.25 }}
                        className="flex items-center justify-between mb-12"
                    >
                        <h2 className="text-4xl font-bold text-[var(--color-text-primary)]">
                            {t('home.stories.title_prefix')} <span className="text-gradient">{t('home.stories.title_suffix')}</span>
                        </h2>
                        <div className="hidden sm:flex items-center gap-3">
                            <button className="w-10 h-10 rounded-full border border-[var(--color-border)] text-[var(--color-text-secondary)] hover:border-[var(--color-accent)] hover:text-[var(--color-accent)] bg-white shadow-sm transition-all flex items-center justify-center">
                                <ChevronRight className="w-5 h-5 rotate-180 rtl:rotate-0" />
                            </button>
                            <button className="w-10 h-10 rounded-full bg-[var(--color-accent)] text-white hover:bg-[var(--color-accent-hover)] shadow-md transition-all flex items-center justify-center">
                                <ChevronRight className="w-5 h-5 rtl:rotate-180" />
                            </button>
                        </div>
                    </motion.div>

                    <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                        <StoryCard
                            title="A Surgeon and a Maverick"
                            subtitle="Sir Magdi Yacoub Speaks"
                            image="https://images.unsplash.com/photo-1559523182-a284c3fb7cff?w=600&h=400&fit=crop"
                            delay={0}
                        />
                        <StoryCard
                            title="From Graduate to Leadership"
                            subtitle="An Alum's Journey"
                            image="https://images.unsplash.com/photo-1560250097-0b93528c311a?w=600&h=400&fit=crop"
                            delay={0.1}
                        />
                        <StoryCard
                            title="Meet the Advocate"
                            subtitle="Jasmine Moussa '03, '05"
                            image="https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=600&h=400&fit=crop"
                            delay={0.2}
                        />
                        <StoryCard
                            title="Trailblazers: Ahmed Samy"
                            subtitle="Class of '04"
                            image="https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=600&h=400&fit=crop"
                            delay={0.3}
                        />
                    </div>
                </div>
            </section>

            {/* Latest News */}
            <section className="py-24 bg-white">
                <div className="container mx-auto px-4 lg:px-8">
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: false, amount: 0.25 }}
                        className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-12"
                    >
                        <h2 className="text-4xl font-bold text-[var(--color-text-primary)] flex items-center gap-4">
                            <span className="p-3 rounded-2xl bg-[var(--color-accent-light)]"><Newspaper className="w-8 h-8 text-[var(--color-accent)]" /></span>
                            {t('home.latest_news.title')}
                        </h2>
                        <Link to="/portal/services" className="group text-[var(--color-accent)] hover:underline flex items-center gap-2 font-bold uppercase tracking-wider text-sm">
                            {t('home.latest_news.view_all')} <ChevronRight className="w-4 h-4 group-hover:translate-x-1 transition-transform rtl:rotate-180 rtl:group-hover:-translate-x-1" />
                        </Link>
                    </motion.div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                        {news?.items.slice(0, 3).map((item, idx) => (
                            <motion.div
                                key={item.id}
                                initial={{ opacity: 0, y: 30 }}
                                whileInView={{ opacity: 1, y: 0 }}
                                viewport={{ once: false, amount: 0.25 }}
                                transition={{ delay: idx * 0.15, type: "spring" }}
                                whileHover={{ y: -10 }}
                                className="group cursor-pointer flex flex-col h-full bg-white rounded-2xl border border-[var(--color-border)] shadow-sm hover:shadow-xl transition-all duration-300 overflow-hidden"
                            >
                                <div className="h-48 relative overflow-hidden">
                                    <img
                                        src={item.coverImageUrl || `https://source.unsplash.com/random/800x600/?${item.category || 'news'}`}
                                        alt={item.title}
                                        className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                                        onError={(e) => {
                                            (e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1504711434969-e33886168f5c?q=80&w=2070&auto=format&fit=crop';
                                        }}
                                    />
                                    <div className="absolute top-4 left-4 z-10 text-white text-[10px] font-bold tracking-widest uppercase bg-[var(--color-accent)] px-3 py-1 rounded-full shadow-lg rtl:left-auto rtl:right-4">
                                        {item.category || 'News'}
                                    </div>
                                </div>

                                <div className="p-6 flex-1 flex flex-col">
                                    <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-3 group-hover:text-[var(--color-accent)] transition-colors line-clamp-2">
                                        {item.title}
                                    </h3>
                                    <p className="text-[var(--color-text-secondary)] text-sm line-clamp-3 mb-4 leading-relaxed flex-1">
                                        {item.content.replace(/<[^>]*>/g, '')}
                                    </p>
                                    <div className="flex items-center justify-between border-t border-[var(--color-border)] pt-4 mt-auto">
                                        <span className="text-xs text-[var(--color-text-muted)] font-medium">
                                            {new Date(item.creationTime || Date.now()).toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' })}
                                        </span>
                                        <ChevronRight className="w-4 h-4 text-[var(--color-accent)] group-hover:translate-x-1 transition-transform rtl:rotate-180 rtl:group-hover:-translate-x-1" />
                                    </div>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                </div>
            </section>

            {/* Upcoming Events */}
            <section className="py-24 bg-slate-50 relative overflow-hidden">
                <div className="container mx-auto px-4 lg:px-8 relative z-10">
                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: false, amount: 0.25 }}
                        className="flex flex-col md:flex-row md:items-center justify-between gap-6 mb-12"
                    >
                        <h2 className="text-4xl font-bold text-[var(--color-text-primary)] flex items-center gap-4">
                            <span className="p-3 rounded-2xl bg-[var(--color-accent-light)]"><Calendar className="w-8 h-8 text-[var(--color-accent)]" /></span>
                            {t('home.upcoming_events.title')}
                        </h2>
                        <Link to="/portal/events">
                            <Button variant="outline" className="group uppercase text-xs tracking-widest px-8">
                                {t('home.upcoming_events.see_all')} <ChevronRight className="w-4 h-4 group-hover:translate-x-1 transition-transform ml-2 rtl:rotate-180 rtl:group-hover:-translate-x-1 rtl:ml-0 rtl:mr-2" />
                            </Button>
                        </Link>
                    </motion.div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                        {events?.items.slice(0, 2).map((event, idx) => (
                            <motion.div
                                key={event.id}
                                initial={{ opacity: 0, x: idx % 2 === 0 ? -30 : 30 }}
                                whileInView={{ opacity: 1, x: 0 }}
                                viewport={{ once: false, amount: 0.25 }}
                                whileHover={{ scale: 1.02 }}
                                transition={{ type: "spring", stiffness: 100 }}
                                className="flex flex-col sm:flex-row bg-white rounded-3xl overflow-hidden shadow-sm border border-[var(--color-border)] group hover:shadow-xl transition-all duration-300"
                            >
                                {/* Event Image */}
                                <div className="h-48 sm:h-auto sm:w-56 relative shrink-0">
                                    <img
                                        src={event.coverImageUrl || `https://source.unsplash.com/random/400x400/?event,campus`}
                                        alt={event.nameEn}
                                        className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                                        onError={(e) => {
                                            (e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1511578314322-379afb476865?q=80&w=2069&auto=format&fit=crop';
                                        }}
                                    />
                                    <div className="absolute top-4 left-4 flex flex-col items-center justify-center w-14 h-14 bg-white rounded-2xl shadow-lg sm:hidden rtl:left-auto rtl:right-4">
                                        <span className="text-[var(--color-accent)] font-black text-lg leading-none">{new Date(event.lastSubscriptionDate).getDate()}</span>
                                        <span className="text-[10px] text-[var(--color-text-muted)] uppercase font-bold tracking-wider">{new Date(event.lastSubscriptionDate).toLocaleString('default', { month: 'short' })}</span>
                                    </div>
                                </div>

                                <div className="p-8 flex flex-col flex-1 relative">
                                    <div className="hidden sm:flex flex-col items-center justify-center w-14 h-14 bg-slate-50 rounded-2xl border border-[var(--color-border)] absolute top-8 right-8 shadow-sm rtl:right-auto rtl:left-8">
                                        <span className="text-[var(--color-accent)] font-black text-lg leading-none">{new Date(event.lastSubscriptionDate).getDate()}</span>
                                        <span className="text-[10px] text-[var(--color-text-muted)] uppercase font-bold tracking-wider">{new Date(event.lastSubscriptionDate).toLocaleString('default', { month: 'short' })}</span>
                                    </div>

                                    <h3 className="text-2xl font-bold text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors mb-2 pr-16 rtl:pr-0 rtl:pl-16">{event.nameEn}</h3>
                                    <p className="text-sm text-[var(--color-text-secondary)] line-clamp-2 mb-6 leading-relaxed">{event.description}</p>
                                    <div className="flex items-center gap-4 text-xs font-bold text-[var(--color-text-muted)] tracking-wider mt-auto">
                                        <span className="flex items-center gap-1.5"><Building2 className="w-4 h-4 text-[var(--color-accent)]" />{event.location}</span>
                                        <div className="w-1 h-1 rounded-full bg-slate-300" />
                                        <span className="text-[var(--color-accent)]">{event.hasFees ? `${event.feeAmount} EGP` : t('home.upcoming_events.free')}</span>
                                    </div>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                </div>
            </section>

            {/* Footer */}
            <footer className="bg-slate-900 py-16 text-center text-white relative z-10 overflow-hidden">
                <div className="absolute inset-0 bg-gradient-to-br from-[var(--color-accent)]/10 to-transparent pointer-events-none" />
                <div className="container mx-auto px-4 lg:px-8 relative z-10">
                    <div className="flex justify-center gap-10 mb-10">
                        <motion.div whileHover={{ y: -5, color: "#2D96D7" }} className="cursor-pointer transition-colors"><Building2 className="w-8 h-8" /></motion.div>
                        <motion.div whileHover={{ y: -5, color: "#2D96D7" }} className="cursor-pointer transition-colors"><GraduationCap className="w-8 h-8" /></motion.div>
                        <motion.div whileHover={{ y: -5, color: "#2D96D7" }} className="cursor-pointer transition-colors"><Users className="w-8 h-8" /></motion.div>
                    </div>
                    <div className="max-w-xs mx-auto mb-8 h-px bg-white/10" />
                    <p className="text-sm text-slate-400 font-medium font-sans">
                        {t('home.footer.copyright', { year: new Date().getFullYear() })} <br />
                        <span className="text-[10px] tracking-widest uppercase opacity-50 mt-2 block">{t('home.footer.tagline')}</span>
                    </p>
                </div>
            </footer>
        </div>
    );
};

interface FeatureCardProps {
    icon: React.ReactNode;
    title: string;
    desc: string;
    to: string;
    color: string;
    exploreText: string;
}

const FeatureCard = ({ icon, title, desc, to, exploreText }: FeatureCardProps) => {
    return (
        <Link to={to} className="block h-full group">
            <motion.div
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: false, amount: 0.25 }}
                whileHover={{ y: -12 }}
                className="bg-white p-10 h-full transition-all duration-300 rounded-3xl border border-[var(--color-border)] shadow-sm hover:shadow-2xl relative overflow-hidden text-center"
            >
                <div className="absolute -right-6 -top-6 w-32 h-32 bg-slate-50 rounded-full blur-2xl group-hover:bg-accent/5 transition-colors" />

                <div className="mb-8 p-6 bg-slate-50 rounded-3xl w-fit mx-auto group-hover:bg-[var(--color-accent-light)] group-hover:scale-110 transition-all duration-500 shadow-sm">
                    {icon}
                </div>
                <h3 className="text-2xl font-bold text-[var(--color-text-primary)] mb-4 group-hover:text-[var(--color-accent)] transition-colors">{title}</h3>
                <p className="text-[var(--color-text-secondary)] leading-relaxed text-sm mb-6">{desc}</p>

                <div className="mt-auto pt-4 flex items-center justify-center gap-2 text-[var(--color-accent)] font-bold text-xs uppercase tracking-widest opacity-0 group-hover:opacity-100 transition-opacity">
                    {exploreText} <ChevronRight className="w-4 h-4 rtl:rotate-180" />
                </div>
            </motion.div>
        </Link>
    );
};

interface StatCardProps {
    value: string;
    label: string;
    delay: number;
    icon?: string;
}

const StatCard = ({ value, label, delay, icon }: StatCardProps) => {
    // Extract numeric part and non-numeric suffix
    const numericValue = parseFloat(value.replace(/[^0-9.]/g, '') || "0");
    const suffix = value.replace(/[0-9.]/g, '');

    const count = useMotionValue(0);
    const rounded = useTransform(count, (latest) => Math.round(latest));
    const ref = useRef<HTMLDivElement>(null);
    const inView = useInView(ref, { once: true });

    useEffect(() => {
        if (inView) {
            animate(count, numericValue, {
                duration: 2,
                ease: "circOut"
            });
        }
    }, [count, inView, numericValue]);

    return (
        <motion.div
            ref={ref}
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }} // Changed to true to count once
            transition={{ delay, duration: 0.8 }}
            whileHover={{ y: -5 }}
            className="text-center group"
        >
            <div className="relative pt-12">
                {icon && (
                    <div className="absolute top-0 left-1/2 -translate-x-1/2 text-4xl group-hover:animate-bounce">
                        {icon}
                    </div>
                )}
                <span className="block text-5xl md:text-6xl font-black text-[var(--color-text-primary)] mb-3 group-hover:text-[var(--color-accent)] transition-colors tracking-tight flex justify-center items-center gap-1">
                    <motion.span>{rounded}</motion.span><span>{suffix}</span>
                </span>
                <p className="text-xs font-bold uppercase tracking-widest text-[var(--color-text-muted)] leading-relaxed max-w-[150px] mx-auto">
                    {label}
                </p>
                <div className="mt-6 w-12 h-1 bg-[var(--color-accent)] mx-auto rounded-full group-hover:w-24 transition-all duration-500" />
            </div>
        </motion.div>
    );
};

interface AlumniCardProps {
    name: string;
    year: string;
    title: string;
    image: string;
    delay: number;
    viewProfileText: string;
    classOfText: string;
}

const AlumniCard = ({ name, year, title, image, delay, viewProfileText, classOfText }: AlumniCardProps) => {
    return (
        <motion.div
            initial={{ opacity: 0, scale: 0.9 }}
            whileInView={{ opacity: 1, scale: 1 }}
            viewport={{ once: false }}
            transition={{ delay, duration: 0.5 }}
            whileHover={{ y: -10 }}
            className="group"
        >
            <div className="relative overflow-hidden rounded-3xl mb-5 aspect-[4/5] shadow-sm group-hover:shadow-2xl transition-all duration-500">
                <img
                    src={image}
                    alt={name}
                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                <div className="absolute bottom-6 left-6 right-6 translate-y-4 opacity-0 group-hover:translate-y-0 group-hover:opacity-100 transition-all duration-300">
                    <Button className="w-full text-xs font-black uppercase tracking-widest py-3">{viewProfileText}</Button>
                </div>
            </div>
            <div className="text-center px-2">
                <h3 className="text-lg font-bold text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors leading-tight">
                    {name}
                </h3>
                <span className="text-[10px] font-black tracking-widest text-[var(--color-accent)] uppercase mt-1 block">{classOfText} 19{year}</span>
                <p className="text-[11px] text-[var(--color-text-secondary)] mt-2 font-medium">{title}</p>
            </div>
        </motion.div>
    );
};

interface StoryCardProps {
    title: string;
    subtitle: string;
    image: string;
    delay: number;
}

const StoryCard = ({ title, subtitle, image, delay }: StoryCardProps) => {
    return (
        <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: false }}
            transition={{ delay, duration: 0.5 }}
            whileHover={{ y: -12 }}
            className="group cursor-pointer"
        >
            <div className="relative overflow-hidden rounded-3xl mb-5 aspect-video shadow-md group-hover:shadow-2xl transition-all duration-500">
                <img
                    src={image}
                    alt={title}
                    className="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent" />

                {/* Play Button Overlay */}
                <div className="absolute inset-0 flex items-center justify-center">
                    <motion.div
                        whileHover={{ scale: 1.1 }}
                        whileTap={{ scale: 0.9 }}
                        className="w-16 h-16 rounded-full bg-white/30 backdrop-blur-xl border border-white/40 flex items-center justify-center group-hover:bg-[var(--color-accent)] group-hover:border-transparent transition-all shadow-xl"
                    >
                        <div className="w-0 h-0 border-t-[8px] border-t-transparent border-l-[14px] border-l-white border-b-[8px] border-b-transparent ml-1" />
                    </motion.div>
                </div>
            </div>
            <div className="px-1 text-center sm:text-left">
                <h3 className="text-base font-black text-[var(--color-text-primary)] group-hover:text-[var(--color-accent)] transition-colors line-clamp-2 leading-snug mb-2 uppercase tracking-tight">
                    {title}
                </h3>
                <p className="text-xs font-bold text-[var(--color-text-muted)] transition-colors italic">"{subtitle}"</p>
            </div>
        </motion.div>
    );
};

const UpcomingEventsList = () => {
    const { t, i18n } = useTranslation();
    const { data } = useQuery({
        queryKey: ['upcoming-events'],
        queryFn: () => eventsService.getList({ maxResultCount: 3, sorting: 'LastSubscriptionDate desc' })
    });

    if (!data?.items.length) return null;

    return (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {data.items.map((event, index) => (
                <motion.div
                    key={event.id}
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: index * 0.1 }}
                >
                    <Card variant="default" className="overflow-hidden flex flex-col h-full group hover:border-[var(--color-accent)]/50 transition-all relative border-[var(--color-border)]">
                        {/* Date Badge */}
                        <div className="h-48 bg-gradient-to-br from-indigo-50 to-slate-100 relative p-4 flex flex-col justify-end border-b border-[var(--color-border)]">
                            <div className="absolute top-4 right-4 bg-white/90 backdrop-blur px-3 py-1 rounded-lg border border-[var(--color-border)] text-xs font-bold text-[var(--color-text-primary)] shadow-sm">
                                {event.hasFees ? `${event.feeAmount} ${t('common.currency')}` : t('events.free_entry')}
                            </div>
                            <div className="absolute top-4 left-4 bg-[var(--color-accent)] text-white w-12 h-14 rounded-lg flex flex-col items-center justify-center font-bold shadow-lg">
                                <span className="text-xs uppercase">{new Date(event.lastSubscriptionDate).toLocaleString(i18n.language, { month: 'short' })}</span>
                                <span className="text-xl">{new Date(event.lastSubscriptionDate).getDate()}</span>
                            </div>
                        </div>

                        <CardContent className="p-6 flex flex-col flex-1">
                            <div className="flex items-start justify-between mb-4">
                                <h3 className="text-xl font-bold text-[var(--color-text-primary)] mb-1 group-hover:text-[var(--color-accent)] transition-colors line-clamp-2">
                                    {event.nameEn}
                                </h3>
                            </div>

                            <div className="space-y-3 pt-4 mt-auto border-t border-[var(--color-border)]">
                                {event.branchName && (
                                    <div className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
                                        <Building2 className="w-4 h-4 text-[var(--color-accent)]" />
                                        <span className="truncate">{event.branchName}</span>
                                    </div>
                                )}
                            </div>

                            <Link to={`/portal/events/${event.id}`} className="mt-6 w-full">
                                <Button variant="outline" className="w-full text-sm group-hover:bg-[var(--color-accent)] group-hover:text-white transition-all">
                                    {t('events.details_btn')} <ChevronRight className="w-4 h-4 ml-2 rtl:rotate-180" />
                                </Button>
                            </Link>
                        </CardContent>
                    </Card>
                </motion.div>
            ))}
        </div>
    );
};

export default PortalHome;
