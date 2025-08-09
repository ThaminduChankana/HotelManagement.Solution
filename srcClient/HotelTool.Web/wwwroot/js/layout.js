// Hotel Management System Layout JavaScript

document.addEventListener("DOMContentLoaded", function () {
  // Header scroll effect
  const header = document.querySelector(".main-header");
  let lastScrollTop = 0;

  window.addEventListener("scroll", function () {
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

    if (scrollTop > lastScrollTop && scrollTop > 100) {
      // Scrolling down
      header.style.transform = "translateY(-100%)";
    } else {
      // Scrolling up
      header.style.transform = "translateY(0)";
    }

    // Add glass effect on scroll
    if (scrollTop > 50) {
      header.style.background = "rgba(0, 0, 0, 0.95)";
      header.style.backdropFilter = "blur(25px)";
    } else {
      header.style.background = "rgba(0, 0, 0, 0.9)";
      header.style.backdropFilter = "blur(20px)";
    }

    lastScrollTop = scrollTop;
  });

  // Active navigation highlighting
  const currentPath = window.location.pathname;
  const navLinks = document.querySelectorAll(".nav-link");

  navLinks.forEach((link) => {
    if (link.getAttribute("href") === currentPath) {
      link.classList.add("active");
    }
  });

  // Smooth dropdown animations
  const dropdownToggle = document.querySelectorAll(".dropdown-toggle");

  dropdownToggle.forEach((toggle) => {
    toggle.addEventListener("click", function (e) {
      const dropdownMenu = this.nextElementSibling;
      if (dropdownMenu) {
        dropdownMenu.style.animation = "fadeInUp 0.3s ease-out";
      }
    });
  });

  // Mobile menu enhancements
  const navbarToggler = document.querySelector(".navbar-toggler");
  const navbarCollapse = document.querySelector(".navbar-collapse");

  if (navbarToggler && navbarCollapse) {
    navbarToggler.addEventListener("click", function () {
      // Add animation class
      setTimeout(() => {
        if (navbarCollapse.classList.contains("show")) {
          navbarCollapse.style.animation = "slideDown 0.3s ease-out";
        }
      }, 10);
    });
  }

  // Footer animations on scroll
  const footerSections = document.querySelectorAll(".footer-section");

  const observerOptions = {
    threshold: 0.1,
    rootMargin: "0px 0px -50px 0px",
  };

  const observer = new IntersectionObserver(function (entries) {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.style.animation = "fadeInUp 0.6s ease-out";
        entry.target.style.opacity = "1";
      }
    });
  }, observerOptions);

  footerSections.forEach((section) => {
    section.style.opacity = "0";
    observer.observe(section);
  });

  // Status indicator animation
  const statusIndicators = document.querySelectorAll(
    ".status-indicator.online"
  );

  statusIndicators.forEach((indicator) => {
    const circle = indicator.querySelector("i");
    if (circle) {
      setInterval(() => {
        circle.style.animation = "pulse 2s ease-in-out";
        setTimeout(() => {
          circle.style.animation = "";
        }, 2000);
      }, 5000);
    }
  });

  // Tooltip functionality for user menu
  const userMenu = document.querySelector(".user-menu");
  if (userMenu) {
    userMenu.setAttribute("title", "User Account Settings");
  }

  // Search functionality (if search box exists)
  const searchInputs = document.querySelectorAll('input[type="search"]');

  searchInputs.forEach((input) => {
    input.addEventListener("focus", function () {
      this.parentElement.style.transform = "scale(1.02)";
      this.parentElement.style.boxShadow =
        "0 5px 20px rgba(102, 126, 234, 0.3)";
    });

    input.addEventListener("blur", function () {
      this.parentElement.style.transform = "scale(1)";
      this.parentElement.style.boxShadow = "none";
    });
  });

  // Notification system (placeholder for future implementation)
  function showNotification(message, type = "info") {
    const notification = document.createElement("div");
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-${
                  type === "success"
                    ? "check-circle"
                    : type === "error"
                    ? "exclamation-circle"
                    : "info-circle"
                }"></i>
                <span>${message}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;

    notification.style.cssText = `
            position: fixed;
            top: 100px;
            right: 20px;
            background: rgba(0, 0, 0, 0.9);
            color: white;
            padding: 1rem;
            border-radius: 8px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            z-index: 9999;
            animation: slideInRight 0.3s ease-out;
            max-width: 300px;
        `;

    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
      if (notification.parentElement) {
        notification.style.animation = "slideOutRight 0.3s ease-out";
        setTimeout(() => notification.remove(), 300);
      }
    }, 5000);
  }

  // Add custom CSS animations
  const style = document.createElement("style");
  style.textContent = `
        @keyframes slideDown {
            from {
                opacity: 0;
                transform: translateY(-10px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        @keyframes pulse {
            0%, 100% { transform: scale(1); opacity: 1; }
            50% { transform: scale(1.2); opacity: 0.7; }
        }
        
        @keyframes slideInRight {
            from {
                opacity: 0;
                transform: translateX(100px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
        
        @keyframes slideOutRight {
            from {
                opacity: 1;
                transform: translateX(0);
            }
            to {
                opacity: 0;
                transform: translateX(100px);
            }
        }
        
        .notification-content {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }
        
        .notification-close {
            background: none;
            border: none;
            color: white;
            cursor: pointer;
            margin-left: auto;
            opacity: 0.7;
            transition: opacity 0.3s ease;
        }
        
        .notification-close:hover {
            opacity: 1;
        }
    `;

  document.head.appendChild(style);

  // Expose notification function globally for use in other scripts
  window.showNotification = showNotification;

  // Performance optimization - lazy load images
  const images = document.querySelectorAll("img[data-src]");

  const imageObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        const img = entry.target;
        img.src = img.dataset.src;
        img.removeAttribute("data-src");
        imageObserver.unobserve(img);
      }
    });
  });

  images.forEach((img) => imageObserver.observe(img));

  console.log("Hotel Management System Layout Initialized");
});
