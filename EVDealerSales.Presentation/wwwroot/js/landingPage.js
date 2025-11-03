/**
 * Landing Page Animations & Effects
 * Includes: Animated Counter, Scroll Animations, and Advanced Hover Effects
 */

(function () {
    'use strict';

    // ============================================
    // 1. ANIMATED STATISTICS COUNTER
    // ============================================
    function animateCounter(element, target, duration = 2000, suffix = '') {
        const startTime = performance.now();
        const startValue = 0;

        function update(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);

            // Easing function (ease-out)
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const current = startValue + (target - startValue) * easeOut;

            element.textContent = Math.floor(current) + suffix;

            if (progress < 1) {
                requestAnimationFrame(update);
            } else {
                element.textContent = target + suffix;
            }
        }

        requestAnimationFrame(update);
    }

    function initCounters() {
        const statNumbers = document.querySelectorAll('.stat-number');
        if (!statNumbers.length) return;

        const observerOptions = {
            threshold: 0.5,
            rootMargin: '0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && !entry.target.classList.contains('counted')) {
                    entry.target.classList.add('counted');
                    const text = entry.target.textContent.trim();

                    // Parse different formats
                    let targetNumber, suffix = '';

                    if (text.includes('+')) {
                        targetNumber = parseInt(text.replace('+', ''));
                        suffix = '+';
                    } else if (text.includes('%')) {
                        targetNumber = parseInt(text.replace('%', ''));
                        suffix = '%';
                    } else if (text.includes('/')) {
                        // For formats like "24/7", just display as is
                        return;
                    } else if (text.includes('-')) {
                        // For formats like "0-60", animate to 60
                        entry.target.textContent = '0';
                        setTimeout(() => {
                            entry.target.textContent = text;
                        }, 500);
                        return;
                    } else {
                        targetNumber = parseInt(text);
                    }

                    if (!isNaN(targetNumber)) {
                        animateCounter(entry.target, targetNumber, 2000, suffix);
                    }

                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        statNumbers.forEach(stat => observer.observe(stat));
    }

    // ============================================
    // 2. SCROLL ANIMATIONS (Fade-in & Slide-up)
    // ============================================
    function initScrollAnimations() {
        const animatedElements = document.querySelectorAll(
            '.stat-card, .benefit-item, .section-title, .section-description, .hero-badge, .hero-title, .hero-description, .hero-actions'
        );

        if (!animatedElements.length) return;

        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    // Staggered animation delay
                    const delay = entry.target.classList.contains('stat-card') ||
                        entry.target.classList.contains('benefit-item')
                        ? index * 100
                        : 0;

                    setTimeout(() => {
                        entry.target.classList.add('animate-in');
                    }, delay);

                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        animatedElements.forEach(el => {
            el.classList.add('animate-on-scroll');
            observer.observe(el);
        });
    }

    // ============================================
    // 3. ADVANCED HOVER EFFECTS
    // ============================================

    // 3a. Glow Effect for Cards
    function initGlowEffect() {
        const cards = document.querySelectorAll('.stat-card, .benefit-item');

        cards.forEach(card => {
            card.addEventListener('mouseenter', function () {
                this.classList.add('glow-active');
            });

            card.addEventListener('mouseleave', function () {
                this.classList.remove('glow-active');
            });
        });
    }

    // 3b. 3D Tilt Effect for Vehicle Cards (if exists on page)
    function init3DTilt() {
        const vehicleCards = document.querySelectorAll('.vehicle-card');
        if (!vehicleCards.length) return;

        vehicleCards.forEach(card => {
            card.addEventListener('mousemove', function (e) {
                const rect = this.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;

                const centerX = rect.width / 2;
                const centerY = rect.height / 2;

                const rotateX = (y - centerY) / 10;
                const rotateY = (centerX - x) / 10;

                this.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-5px)`;
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateY(0)';
            });
        });
    }

    // 3c. Ripple Effect for Buttons
    function initRippleEffect() {
        const buttons = document.querySelectorAll('.btn');

        buttons.forEach(button => {
            button.addEventListener('click', function (e) {
                const ripple = document.createElement('span');
                ripple.classList.add('ripple');

                const rect = this.getBoundingClientRect();
                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - size / 2;
                const y = e.clientY - rect.top - size / 2;

                ripple.style.width = ripple.style.height = size + 'px';
                ripple.style.left = x + 'px';
                ripple.style.top = y + 'px';

                this.appendChild(ripple);

                setTimeout(() => ripple.remove(), 600);
            });
        });
    }

    // ============================================
    // 4. SMOOTH SCROLL BEHAVIOR
    // ============================================
    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#') return;

                e.preventDefault();
                const target = document.querySelector(href);

                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // ============================================
    // 5. PERFORMANCE OPTIMIZATION
    // ============================================
    function reduceMotionCheck() {
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        if (prefersReducedMotion) {
            document.body.classList.add('reduce-motion');
        }
    }

    // ============================================
    // INITIALIZATION
    // ============================================
    function init() {
        // Check for reduced motion preference
        reduceMotionCheck();

        // Initialize all effects (removed initParallax)
        initCounters();
        initScrollAnimations();
        initGlowEffect();
        init3DTilt();
        initRippleEffect();
        initSmoothScroll();

        console.log('🚀 Landing page animations initialized');
    }

    // Run on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();