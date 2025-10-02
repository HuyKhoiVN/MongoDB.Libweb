// Common JavaScript for interactivity in the template
// Includes smooth scroll, button hover effects, and simple animations

document.addEventListener('DOMContentLoaded', () => {
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            document.querySelector(this.getAttribute('href')).scrollIntoView({
                behavior: 'smooth'
            });
        });
    });

    // Button hover animation (scale effect already in CSS, add JS for more)
    const buttons = document.querySelectorAll('.btn-primary');
    buttons.forEach(btn => {
        btn.addEventListener('mouseover', () => {
            btn.style.transform = 'scale(1.05)';
        });
        btn.addEventListener('mouseout', () => {
            btn.style.transform = 'scale(1)';
        });
    });

    // Fade-in animation for sections on scroll
    const sections = document.querySelectorAll('section');
    const options = {
        threshold: 0.1
    };
    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                observer.unobserve(entry.target);
            }
        });
    }, options);

    sections.forEach(section => {
        section.classList.add('hidden'); // Initial hidden class
        observer.observe(section);
    });
});

// Add this to CSS for fade-in: .hidden { opacity: 0; transform: translateY(20px); transition: all 0.5s; } .fade-in { opacity: 1; transform: translateY(0); }