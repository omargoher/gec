import { Ic as PLATFORM_ID, Ui as setClassMetadata, dr as Service, io as ɵɵdefineService, ol as inject } from "./core-DK4zC9WD.js";
import { i as isPlatformBrowser } from "./common-BlhNZJxu.js";
//#region node_modules/@angular/cdk/fesm2022/_platform-chunk.mjs
var hasV8BreakIterator;
try {
	hasV8BreakIterator = typeof Intl !== "undefined" && Intl.v8BreakIterator;
} catch {
	hasV8BreakIterator = false;
}
var Platform = class Platform {
	_platformId = inject(PLATFORM_ID);
	isBrowser = this._platformId ? isPlatformBrowser(this._platformId) : typeof document === "object" && !!document;
	EDGE = this.isBrowser && /(edge)/i.test(navigator.userAgent);
	TRIDENT = this.isBrowser && /(msie|trident)/i.test(navigator.userAgent);
	BLINK = this.isBrowser && !!(window.chrome || hasV8BreakIterator) && typeof CSS !== "undefined" && !this.EDGE && !this.TRIDENT;
	WEBKIT = this.isBrowser && /AppleWebKit/i.test(navigator.userAgent) && !this.BLINK && !this.EDGE && !this.TRIDENT;
	IOS = this.isBrowser && /iPad|iPhone|iPod/.test(navigator.userAgent) && !("MSStream" in window);
	FIREFOX = this.isBrowser && /(firefox|minefield)/i.test(navigator.userAgent);
	ANDROID = this.isBrowser && /android/i.test(navigator.userAgent) && !this.TRIDENT;
	SAFARI = this.isBrowser && /safari/i.test(navigator.userAgent) && this.WEBKIT;
	static ɵfac = function Platform_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || Platform)();
	};
	static ɵprov = /* @__PURE__ */ ɵɵdefineService({
		token: Platform,
		factory: Platform.ɵfac
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(Platform, [{ type: Service }], null, null);
})();
//#endregion
export { Platform as t };
