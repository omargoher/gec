import { n as __toESM } from "./rolldown-runtime-FDOR9p9I.js";
import { El as ɵɵdefineInjector, Fn as Injectable, Pc as NgZone, Pn as Inject, Ti as performanceMarkFeature, Tl as ɵɵdefineInjectable, Ui as setClassMetadata, ar as RendererFactory2, cc as ANIMATION_MODULE_TYPE, kl as ɵɵinject, no as ɵɵdefineNgModule, ol as inject, pc as DOCUMENT, qn as NgModule } from "./core-DK4zC9WD.js";
import "./common-BlhNZJxu.js";
import { r as DomRendererFactory2 } from "./_dom_renderer-chunk-B6SgvKyr.js";
import { r as BrowserModule } from "./_browser-chunk-BrT0irzu.js";
import { t as require_platform_browser } from "./platform-browser-P-nwT-yV.js";
//#region node_modules/@angular/platform-browser/fesm2022/animations.mjs
/**
* @license Angular v22.1.3
* (c) 2010-2026 Google LLC. https://angular.dev/
* License: MIT
*/
var import_platform_browser = /* @__PURE__ */ __toESM(require_platform_browser(), 1);
var InjectableAnimationEngine = class InjectableAnimationEngine extends import_platform_browser.ɵAnimationEngine {
	constructor(doc, driver, normalizer) {
		super(doc, driver, normalizer);
	}
	ngOnDestroy() {
		this.flush();
	}
	static ɵfac = function InjectableAnimationEngine_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || InjectableAnimationEngine)(ɵɵinject(DOCUMENT), ɵɵinject(import_platform_browser.AnimationDriver), ɵɵinject(import_platform_browser.ɵAnimationStyleNormalizer));
	};
	static ɵprov = /* @__PURE__ */ ɵɵdefineInjectable({
		token: InjectableAnimationEngine,
		factory: InjectableAnimationEngine.ɵfac
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(InjectableAnimationEngine, [{ type: Injectable }], () => [
		{
			type: Document,
			decorators: [{
				type: Inject,
				args: [DOCUMENT]
			}]
		},
		{ type: import_platform_browser.AnimationDriver },
		{ type: import_platform_browser.ɵAnimationStyleNormalizer }
	], null);
})();
function instantiateDefaultStyleNormalizer() {
	return new import_platform_browser.ɵWebAnimationsStyleNormalizer();
}
function instantiateRendererFactory() {
	return new import_platform_browser.ɵAnimationRendererFactory(inject(DomRendererFactory2), inject(import_platform_browser.ɵAnimationEngine), inject(NgZone));
}
var SHARED_ANIMATION_PROVIDERS = [
	{
		provide: import_platform_browser.ɵAnimationStyleNormalizer,
		useFactory: instantiateDefaultStyleNormalizer
	},
	{
		provide: import_platform_browser.ɵAnimationEngine,
		useClass: InjectableAnimationEngine
	},
	{
		provide: RendererFactory2,
		useFactory: instantiateRendererFactory
	}
];
var BROWSER_NOOP_ANIMATIONS_PROVIDERS = [
	{
		provide: import_platform_browser.AnimationDriver,
		useClass: import_platform_browser.NoopAnimationDriver
	},
	{
		provide: ANIMATION_MODULE_TYPE,
		useValue: "NoopAnimations"
	},
	...SHARED_ANIMATION_PROVIDERS
];
var BROWSER_ANIMATIONS_PROVIDERS = [
	{
		provide: import_platform_browser.AnimationDriver,
		useFactory: () => new import_platform_browser.ɵWebAnimationsDriver()
	},
	{
		provide: ANIMATION_MODULE_TYPE,
		useFactory: () => "BrowserAnimations"
	},
	...SHARED_ANIMATION_PROVIDERS
];
var BrowserAnimationsModule = class BrowserAnimationsModule {
	static withConfig(config) {
		return {
			ngModule: BrowserAnimationsModule,
			providers: config.disableAnimations ? BROWSER_NOOP_ANIMATIONS_PROVIDERS : BROWSER_ANIMATIONS_PROVIDERS
		};
	}
	static ɵfac = function BrowserAnimationsModule_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || BrowserAnimationsModule)();
	};
	static ɵmod = /* @__PURE__ */ ɵɵdefineNgModule({
		type: BrowserAnimationsModule,
		exports: [BrowserModule]
	});
	static ɵinj = /* @__PURE__ */ ɵɵdefineInjector({
		providers: BROWSER_ANIMATIONS_PROVIDERS,
		imports: [BrowserModule]
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(BrowserAnimationsModule, [{
		type: NgModule,
		args: [{
			exports: [BrowserModule],
			providers: BROWSER_ANIMATIONS_PROVIDERS
		}]
	}], null, null);
})();
function provideAnimations() {
	performanceMarkFeature("NgEagerAnimations");
	return [...BROWSER_ANIMATIONS_PROVIDERS];
}
var NoopAnimationsModule = class NoopAnimationsModule {
	static ɵfac = function NoopAnimationsModule_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || NoopAnimationsModule)();
	};
	static ɵmod = /* @__PURE__ */ ɵɵdefineNgModule({
		type: NoopAnimationsModule,
		exports: [BrowserModule]
	});
	static ɵinj = /* @__PURE__ */ ɵɵdefineInjector({
		providers: BROWSER_NOOP_ANIMATIONS_PROVIDERS,
		imports: [BrowserModule]
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(NoopAnimationsModule, [{
		type: NgModule,
		args: [{
			exports: [BrowserModule],
			providers: BROWSER_NOOP_ANIMATIONS_PROVIDERS
		}]
	}], null, null);
})();
function provideNoopAnimations() {
	return [...BROWSER_NOOP_ANIMATIONS_PROVIDERS];
}
//#endregion
export { ANIMATION_MODULE_TYPE, BrowserAnimationsModule, NoopAnimationsModule, provideAnimations, provideNoopAnimations, InjectableAnimationEngine as ɵInjectableAnimationEngine };
