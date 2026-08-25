import { Fn as Injectable, Tl as ɵɵdefineInjectable, Ui as setClassMetadata, dr as Service, io as ɵɵdefineService, kl as ɵɵinject } from "./core-DK4zC9WD.js";
//#region node_modules/@angular/common/fesm2022/_xhr-chunk.mjs
/**
* @license Angular v22.1.3
* (c) 2010-2026 Google LLC. https://angular.dev/
* License: MIT
*/
function parseCookieValue(cookieStr, name) {
	name = encodeURIComponent(name);
	for (const cookie of cookieStr.split(";")) {
		const eqIndex = cookie.indexOf("=");
		const [cookieName, cookieValue] = eqIndex == -1 ? [cookie, ""] : [cookie.slice(0, eqIndex), cookie.slice(eqIndex + 1)];
		if (cookieName.trim() !== name) continue;
		let value = cookieValue;
		try {
			value = decodeURIComponent(cookieValue);
		} catch {}
		if (value.length > 1 && value[0] === "\"" && value[value.length - 1] === "\"") value = value.slice(1, -1);
		return value;
	}
	return null;
}
var BrowserXhr = class BrowserXhr {
	build() {
		return new XMLHttpRequest();
	}
	static ɵfac = function BrowserXhr_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || BrowserXhr)();
	};
	static ɵprov = /* @__PURE__ */ ɵɵdefineService({
		token: BrowserXhr,
		factory: BrowserXhr.ɵfac
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(BrowserXhr, [{ type: Service }], null, null);
})();
var XhrFactory = class XhrFactory {
	static ɵfac = function XhrFactory_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || XhrFactory)();
	};
	static ɵprov = /* @__PURE__ */ ɵɵdefineInjectable({
		token: XhrFactory,
		factory: function XhrFactory_Factory(__ngFactoryType__) {
			let __ngConditionalFactory__ = null;
			if (__ngFactoryType__) __ngConditionalFactory__ = new (__ngFactoryType__ || XhrFactory)();
			else __ngConditionalFactory__ = ɵɵinject(BrowserXhr);
			return __ngConditionalFactory__;
		},
		providedIn: "root"
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(XhrFactory, [{
		type: Injectable,
		args: [{
			providedIn: "root",
			useExisting: BrowserXhr
		}]
	}], null, null);
})();
//#endregion
export { parseCookieValue as n, XhrFactory as t };
