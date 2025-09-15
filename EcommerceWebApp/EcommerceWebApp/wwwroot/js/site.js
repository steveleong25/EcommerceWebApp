// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

async function updateCartBadge() {
    try {
        let response = await fetch('/api/cart/count');
        let data = await response.json();

        let badge = document.getElementById('cartBadge');
        if (data.count > 0) {
            badge.style.display = 'inline-block';
            badge.textContent = data.count;
        } else {
            badge.style.display = 'none';
        }
    } catch (e) {
        console.error("Error fetching cart count", e);
    }
}

// update on page load
updateCartBadge();

function filterProducts() {
    let input = document.getElementById("searchInput").value.toLowerCase();
    let products = document.querySelectorAll(".product-card");

    products.forEach(product => {
        let title = product.querySelector(".card-title").textContent.toLowerCase();
        if (title.includes(input)) {
            product.style.display = "block";
        } else {
            product.style.display = "none";
        }
    });
}