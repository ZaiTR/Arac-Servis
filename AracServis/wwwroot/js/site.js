document.addEventListener('DOMContentLoaded', () => {

    // Navbar Mobile Menu Handling
    const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
    const nav = document.querySelector('nav');

    if (mobileMenuBtn && nav) {
        mobileMenuBtn.addEventListener('click', () => {
            nav.classList.toggle('active');

            // Icon animation
            const icon = mobileMenuBtn.querySelector('i');
            if (icon) {
                if (nav.classList.contains('active')) {
                    icon.classList.remove('fa-bars');
                    icon.classList.add('fa-times');
                } else {
                    icon.classList.remove('fa-times');
                    icon.classList.add('fa-bars');
                }
            }
        });
    }

    // Navbar Scroll Effect
    window.addEventListener('scroll', () => {
        const header = document.querySelector('header');
        if (header) {
            header.classList.toggle('scrolled', window.scrollY > 50);
        }
    });

    // Scroll Animations
    const observerOptions = {
        threshold: 0.2
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('show');
            }
        });
    }, observerOptions);

    const elements = document.querySelectorAll(".scroll-animate");
    elements.forEach(el => observer.observe(el));

    // Admin Panel / Dashboard Dummy Data (Optional, ensuring it doesn't break anything)
    const servisContainer = document.querySelector('.servis-container');
    if (servisContainer) {
        // Only run if container exists
        console.log("Servis container found.");
    }
});