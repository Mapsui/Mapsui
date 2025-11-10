// Handle tab switching on small screens
document.addEventListener('DOMContentLoaded', () => {
    const tabButtons = document.querySelectorAll('.tab-btn');
    const panels = document.querySelectorAll('.panel');

    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            // Get the target panel ID from data attribute
            const targetPanelId = button.getAttribute('data-panel');

            // Remove active class from all buttons and panels
            tabButtons.forEach(btn => btn.classList.remove('active'));
            panels.forEach(panel => panel.classList.remove('active'));

            // Add active class to clicked button and corresponding panel
            button.classList.add('active');
            document.getElementById(targetPanelId).classList.add('active');
        });
    });
});
