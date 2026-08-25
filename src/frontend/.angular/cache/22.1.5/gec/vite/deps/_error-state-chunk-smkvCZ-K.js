import { Ui as setClassMetadata, dr as Service, io as ɵɵdefineService, ll as isSignal } from "./core-DK4zC9WD.js";
//#region node_modules/@angular/material/fesm2022/_error-options-chunk.mjs
var ShowOnDirtyErrorStateMatcher = class ShowOnDirtyErrorStateMatcher {
	isErrorState(control, form) {
		return !!(control && control.invalid && (control.dirty || form && form.submitted));
	}
	isSignalErrorState(field) {
		if (!field) return false;
		const invalid = field().invalid();
		const dirty = field().dirty();
		return invalid && dirty;
	}
	static ɵfac = function ShowOnDirtyErrorStateMatcher_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || ShowOnDirtyErrorStateMatcher)();
	};
	static ɵprov = /* @__PURE__ */ ɵɵdefineService({
		token: ShowOnDirtyErrorStateMatcher,
		factory: ShowOnDirtyErrorStateMatcher.ɵfac,
		autoProvided: false
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ShowOnDirtyErrorStateMatcher, [{
		type: Service,
		args: [{ autoProvided: false }]
	}], null, null);
})();
var ErrorStateMatcher = class ErrorStateMatcher {
	isErrorState(control, form) {
		return !!(control && control.invalid && (control.touched || form && form.submitted));
	}
	isSignalErrorState(field) {
		if (!field) return false;
		const invalid = field().invalid();
		const touched = field().touched();
		return invalid && touched;
	}
	static ɵfac = function ErrorStateMatcher_Factory(__ngFactoryType__) {
		return new (__ngFactoryType__ || ErrorStateMatcher)();
	};
	static ɵprov = /* @__PURE__ */ ɵɵdefineService({
		token: ErrorStateMatcher,
		factory: ErrorStateMatcher.ɵfac
	});
};
(() => {
	(typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(ErrorStateMatcher, [{ type: Service }], null, null);
})();
//#endregion
//#region node_modules/@angular/material/fesm2022/_error-state-chunk.mjs
var _ErrorStateTracker = class {
	_defaultMatcher;
	_parentFormGroup;
	_parentForm;
	_stateChanges;
	errorState = false;
	matcher;
	ngControl;
	formField;
	constructor(_defaultMatcher, directive, _parentFormGroup, _parentForm, _stateChanges) {
		this._defaultMatcher = _defaultMatcher;
		this._parentFormGroup = _parentFormGroup;
		this._parentForm = _parentForm;
		this._stateChanges = _stateChanges;
		if (!directive) this.ngControl = this.formField = null;
		else if (isSignal(directive.field) && !directive.updateValueAndValidity) {
			this.formField = directive;
			this.ngControl = null;
		} else {
			this.formField = null;
			this.ngControl = directive;
		}
	}
	updateErrorState() {
		const oldState = this.errorState;
		const newState = this._getCurrentErrorState(this.matcher || this._defaultMatcher);
		if (newState !== oldState) {
			this.errorState = newState;
			this._stateChanges.next();
		}
	}
	_getCurrentErrorState(matcher) {
		if (this.formField && matcher?.isSignalErrorState) return matcher.isSignalErrorState(this.formField.field()) ?? false;
		const parent = this._parentFormGroup || this._parentForm;
		const control = this.ngControl ? this.ngControl.control : null;
		return matcher?.isErrorState(control, parent) ?? false;
	}
};
//#endregion
export { ErrorStateMatcher as n, _ErrorStateTracker as t };
