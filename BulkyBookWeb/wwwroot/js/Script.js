// $(window).on("load", function () {
//     // home section slideshow
//     let slideIndex = $(".slideshow.active").index();
//     const slideLen = $(".slideshow").length;
//     function slideShow() {
//       // console.log(slideIndex);
//       $(".slideshow").removeClass("active").eq(slideIndex).addClass("active");
//       if (slideIndex == slideLen - 1) {
//         slideIndex = 0;
//       } else {
//         slideIndex++;
//       }
//       setTimeout(slideShow, 5000);
//     }
//     slideShow();
//   });

document.getElementById("accountBtn").addEventListener("click", function () {
	var loginForm = document.getElementById("loginForm");
	var registerForm = document.getElementById("registerForm");

	if (loginForm.classList.contains("hidden")) {
		loginForm.classList.remove("hidden");
		registerForm.classList.add("hidden");
	} else {
		loginForm.classList.add("hidden");
		registerForm.classList.remove("hidden");
	}
});

// Slick slider
$(document).ready(function () {
	$('.partner-logos').slick({
		slidesToShow: 6,
		slidesToScroll: 1,
		autoplay: true,
		autoplaySpeed: 1500,
		arrows: false,
		dots: false,
		pauseOnHover: false,
		responsive: [{
			breakpoint: 768,
			setting: {
				slidesToShow: 4
			}
		}
		]
	});
});